using SwaggerTest.Core;
using SwaggerTest.Core.Rules;

namespace SwaggerTest.Tests;

public class ScanRunnerTests
{
    private const string LeakySpec = """
    {
      "openapi": "3.0.1",
      "info": { "title": "T", "version": "1" },
      "paths": { "/me": { "get": { "responses": { "200": {
        "description": "ok",
        "content": { "application/json": { "schema": {
          "type": "object",
          "properties": { "apiKey": { "type": "string" } } } } } } } } } }
    }
    """;

    private const string CleanSpec = """
    {
      "openapi": "3.0.1",
      "info": { "title": "T", "version": "1" },
      "paths": { "/ping": { "get": { "responses": { "200": {
        "description": "ok",
        "content": { "application/json": { "schema": {
          "type": "object",
          "properties": { "status": { "type": "string" } } } } } } } } } }
    }
    """;

    // Pass null to simulate a missing file.
    private static async Task<(int Code, string Out, string Err)> RunAsync(string? specContent)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        try
        {
            if (specContent is not null)
                await File.WriteAllTextAsync(path, specContent);

            var output = new StringWriter();
            var error = new StringWriter();
            var runner = new ScanRunner(new Scanner([new SensitiveFieldRule()]), output, error);

            var code = await runner.RunAsync(path);
            return (code, output.ToString(), error.ToString());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task Clean_spec_exits_with_success()
    {
        var (code, _, err) = await RunAsync(CleanSpec);

        Assert.Equal(ExitCodes.Success, code);
        Assert.Empty(err);
    }

    [Fact]
    public async Task Leaky_spec_exits_with_findings_and_prints_json_to_stdout()
    {
        var (code, output, err) = await RunAsync(LeakySpec);

        Assert.Equal(ExitCodes.FindingsFound, code);
        Assert.Contains("SENSITIVE_FIELD", output);
        Assert.Empty(err);
    }

    [Fact]
    public async Task Missing_file_exits_with_tool_error_and_prints_to_stderr_only()
    {
        var (code, output, err) = await RunAsync(null);

        Assert.Equal(ExitCodes.ToolError, code);
        Assert.Empty(output);
        Assert.Contains("File not found", err);
    }
}