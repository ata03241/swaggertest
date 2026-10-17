using SwaggerTest.Core.Spec;

namespace SwaggerTest.Tests;

public class SpecLoaderTests
{
    private const string ValidSpec = """
    {
      "openapi": "3.0.1",
      "info": { "title": "Test API", "version": "1.0" },
      "paths": {
        "/users": {
          "get": { "responses": { "200": { "description": "ok" } } }
        }
      }
    }
    """;

    [Fact]
    public void LoadFromText_parses_a_valid_spec()
    {
        var result = SpecLoader.LoadFromText(ValidSpec);

        Assert.True(result.Success);
        Assert.NotNull(result.Document);
        Assert.True(result.Document!.Paths.ContainsKey("/users"));
    }

    [Fact]
    public void LoadFromText_returns_errors_for_empty_input()
    {
        var result = SpecLoader.LoadFromText("   ");

        Assert.False(result.Success);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void LoadFromText_does_not_throw_on_invalid_json()
    {
        var result = SpecLoader.LoadFromText("{ not json");

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task LoadFromFileAsync_reports_missing_file()
    {
        var result = await SpecLoader.LoadFromFileAsync("does-not-exist.json");

        Assert.False(result.Success);
        Assert.Contains("File not found", result.Errors[0]);
    }
}