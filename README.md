# DeployTrigger

A trivial app that runs scripts in response to HTTP requests.

## Configuration

The following variables may be set, either in `appsettings.json` under a `DeployTrigger` key, or as environment variables with a `DeployTrigger__` prefix:

* `Token` must be specified in all request using an `Authentication: Bearer <token>` header
* `Directory` is where the scripts to execute are located
* `Extension` is an extension to append to file names (defaults to empty)

## Usage Example

Given the following configuration:

```json
{
  "DeployTrigger": {
    "Token": "use-a-secure-token-here",
    "Directory": "/srv/DeployTrigger/scripts",
    "Extension": ".sh"
  }
}
```

Calling `https://example.com/myproject?arg=1.2.3` will execute `/tmp/dt-scripts/myproject.sh v1.2.3`.

## Setup (Linux, systemd)

1. Get an executable from [GitHub Releases](https://github.com/K-Wojciechowski/DeployTrigger/releases) or build your own using `dotnet publish -o dist`
2. Put the executable in `/srv/DeployTrigger/bin`
3. Copy the `deploytrigger.service` file to `/etc/systemd/system/deploytrigger.service`
4. Create `/srv/DeployTrigger/.env`, define `DeployTrigger__Token`, `DeployTrigger__Directory` (e.g. `"/srv/DeployTrigger/scripts"`), `ASPNETCORE_URLS` (e.g. `"http://127.0.0.1:3838"`)
5. Create a user and group named `deploytrigger`
6. Run `systemctl daemon-reload; systemctl enable deploytrigger; systemctl start deploytrigger`
7. Configure nginx as a reverse proxy

## MIT License

Copyright (c) 2025 Krzysztof Wojciechowski

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
