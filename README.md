# swaggertest

A small command-line tool that reads an OpenAPI spec and tells you if any endpoint returns something it probably shouldn't, like a password hash or an API key.

I built it because AI tools can write a working REST API in a few minutes, and it's easy to end up with an endpoint that hands back the whole database row without anyone noticing. The spec usually shows it, so the tool checks the spec.

It's early. Right now it has one check, it only reads JSON specs, and it never calls your API.

## Install

It isn't on NuGet yet, so you build it yourself. You need the .NET 10 SDK.

```bash
git clone https://github.com/ata03241/swaggertest.git
cd swaggertest
dotnet pack src/SwaggerTest.Cli -c Release -o nupkg
dotnet tool install --global SwaggerTest.Cli --add-source ./nupkg --version 0.1.0-alpha.1
```

## Use it

```bash
swaggertest scan path/to/openapi.json
```

The report is JSON and goes to stdout. Error messages go to stderr, so you can pipe the report into other tools without it getting mixed up.

The exit code tells you what happened:

| Code | Meaning |
|------|---------|
| 0 | Nothing serious found |
| 1 | Found at least one high-severity problem |
| 2 | The tool couldn't run (file missing, spec unreadable, bad arguments) |

I kept 1 and 2 apart on purpose. "Your API has a problem" and "my tool broke" shouldn't look the same in a CI pipeline.

## Try it

The repo has a deliberately leaky API in `demo/DemoApi` and a spec generated from it in `samples/demo.json`:

```bash
swaggertest scan samples/demo.json
```

You get four findings. This is the first one (the real output has all four):

```json
{
  "ruleId": "SENSITIVE_FIELD",
  "severity": "high",
  "endpoint": "GET /users",
  "message": "Response 200 exposes sensitive field '$[].passwordHash'.",
  "fixHint": "Remove it from the response model, or map to a DTO that excludes it."
}
```

In `$[].passwordHash`, `$` is the response body and `[]` means every item in a list. A plain `$.apiKey` is a field on a single object.

The demo API is insecure on purpose. Please don't deploy it. All the secrets in it are fake.

## What it checks

One rule so far, `SENSITIVE_FIELD`. It flags response fields named things like `passwordHash`, `apiKey`, `clientSecret`, `token` or `ssn`. It follows `$ref`, arrays and `allOf`/`oneOf`/`anyOf`, so a field buried inside a nested schema still gets found.

It only looks at responses. A login request that contains `password` is normal, so request bodies are ignored.

Names are matched exactly (ignoring case, `_` and `-`). I did that so `nextPageToken` doesn't get flagged, but the downside is that odd names like `pwd` or `authToken` slip through.

## What it doesn't do

- It never sends a request. It reads the spec, and a spec can say something different from what the API really returns. If the code returns a field the spec doesn't mention, this tool won't see it.
- It doesn't check authentication, input handling, or anything else yet.
- No YAML.
- A clean report only means this one check found nothing. It doesn't mean your API is safe.

## How it's built

The rules and scanning logic live in `SwaggerTest.Core`. The command-line part in `SwaggerTest.Cli` is thin on purpose, so everything important can be tested without starting a process. Tests use xUnit and run in GitHub Actions on every pull request.


## What's next

Checking running APIs (localhost only unless you say otherwise), a check for endpoints with no authentication, a check for request bodies that accept fields like `isAdmin`, YAML support, and publishing to NuGet.

## Development

```bash
dotnet test
```
