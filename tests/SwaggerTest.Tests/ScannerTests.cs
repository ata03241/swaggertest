using SwaggerTest.Core;
using SwaggerTest.Core.Rules;
using SwaggerTest.Core.Spec;

namespace SwaggerTest.Tests;

public class ScannerTests
{
    [Fact]
    public void Scan_collects_findings_from_all_rules()
    {
        var load = SpecLoader.LoadFromText("""
        {
          "openapi": "3.0.1",
          "info": { "title": "T", "version": "1" },
          "paths": { "/me": { "get": { "responses": { "200": {
            "description": "ok",
            "content": { "application/json": { "schema": {
              "type": "object",
              "properties": { "apiKey": { "type": "string" } } } } } } } } } }
        }
        """);

        var report = new Scanner([new SensitiveFieldRule()])
            .Scan(load.Document!, "test.json");

        Assert.Equal(1, report.Total);
        Assert.True(report.HasHighSeverity);
    }
}