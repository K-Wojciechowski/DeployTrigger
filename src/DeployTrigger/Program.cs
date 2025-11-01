using System.Diagnostics;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddHttpLogging(o => o.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.Response);

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSimpleConsole(options => {
  options.IncludeScopes = true;
  options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
}));

var app = builder.Build();

app.UseHttpLogging();

var token = app.Configuration["DeployTrigger:Token"];
var directory = app.Configuration["DeployTrigger:Directory"];
var extension = app.Configuration["DeployTrigger:Extension"] ?? "";

if (token == null) {
  throw new Exception("Token is not configured");
}

if (directory == null) {
  throw new Exception("Directory is not configured");
}

var expectedAuth = "Bearer " + app.Configuration["DeployTrigger:Token"];

var projectRegex = Constants.ProjectRegex();

app.MapGet("/{project}", async (string project, [FromQuery] string? argument, HttpContext context) => {
  var authHeader = context.Request.Headers.Authorization;
  if (authHeader.Count != 1 || authHeader[0] != expectedAuth) {
    app.Logger.LogDebug("Invalid Authorization header for project '{Project}' and argument '{Argument}", project,
        argument);
    return Results.Unauthorized();
  }

  if (project.Length == 0 || project.Length > 50 || !projectRegex.IsMatch(project) ||
      (argument != null && (argument.Contains('"') || argument.Length > 50))) {
    app.Logger.LogDebug("Invalid values for project '{Project}' and argument '{Argument}", project, argument);
    return Results.BadRequest();
  }

  var scriptPath = Path.Join(directory, project + extension);

  if (!File.Exists(scriptPath)) {
    return Results.NotFound();
  }

  var startInfo = new ProcessStartInfo(scriptPath) { RedirectStandardOutput = true, RedirectStandardError = true };

  if (!string.IsNullOrEmpty(argument)) {
    startInfo.ArgumentList.Add(argument);
  }

  app.Logger.LogInformation("Running script '{ScriptPath}' with argument '{Argument}'", scriptPath, argument);

  var process = Process.Start(startInfo);

  if (process == null) {
    app.Logger.LogError("Process.Start returned null");
    return Results.InternalServerError();
  }

  await process.WaitForExitAsync().ConfigureAwait(false);

  var stdout = (await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false)).Trim();
  var stderr = (await process.StandardError.ReadToEndAsync().ConfigureAwait(false)).Trim();

  if (!string.IsNullOrWhiteSpace(stdout)) {
    app.Logger.LogDebug("Script '{ScriptPath}' with argument '{Argument}' output:\n{Output}", scriptPath, argument,
        stdout);
  }

  if (!string.IsNullOrWhiteSpace(stdout)) {
    app.Logger.LogDebug("Script '{ScriptPath}' with argument '{Argument}' error output:\n{ErrorOutput}", scriptPath,
        argument, stderr);
  }

  app.Logger.LogInformation("Script '{ScriptPath}' with argument '{Argument}' exited with code '{ExitCode}'",
      scriptPath, argument, process.ExitCode);
  return process.ExitCode == 0 ? Results.Ok() : Results.InternalServerError(process.ExitCode);
});

app.Run();
