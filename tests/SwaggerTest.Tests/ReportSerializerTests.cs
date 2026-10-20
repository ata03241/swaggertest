using SwaggerTest.Core;
using SwaggerTest.Core.Models;

namespace SwaggerTest.Tests;

public class ReportSerializerTests
{
    [Fact]
    public void ToJson_uses_camelCase_and_string_severity()
    {
        var report = new ScanReport("api.json",
        [
            new Finding("SENSITIVE_FIELD", Severity.High, "GET /users",
                "Response contains 'passwordHash'", "Remove it from the response DTO")
        ]);

        var json = ReportSerializer.ToJson(report);

        Assert.Contains("\"ruleId\": \"SENSITIVE_FIELD\"", json);
        Assert.Contains("\"severity\": \"high\"", json);
        Assert.Contains("\"hasHighSeverity\": true", json);
    }

    [Fact]
    public void HasHighSeverity_is_false_when_no_high_findings()
    {
        var report = new ScanReport("api.json",
            [new Finding("X", Severity.Low, "GET /a", "minor")]);

        Assert.False(report.HasHighSeverity);
    }

    [Fact]
    public void ToJson_does_not_escape_apostrophes()
    {
        var report = new ScanReport("api.json",
            [new Finding("X", Severity.Low, "GET /a", "field 'x'")]);

        var json = ReportSerializer.ToJson(report);

        Assert.Contains("field 'x'", json);
    }
}