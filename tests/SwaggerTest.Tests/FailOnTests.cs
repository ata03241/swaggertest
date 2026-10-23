using Microsoft.OpenApi;
using SwaggerTest.Core;
using SwaggerTest.Core.Models;
using SwaggerTest.Core.Rules;

namespace SwaggerTest.Tests;

public class FailOnTests
{
    private sealed class FixedRule(Severity severity) : IRule
    {
        public string Id => "FIXED"; // fixed ID for testing

        public IEnumerable<Finding> Evaluate(OpenApiDocument document) =>
            [new Finding("FIXED", severity, "GET /x", "test finding")];
    }

    // minimal OpenAPI spec to use for testing.
    private const string Spec = """
    {
      "openapi": "3.0.1",
      "info": { "title": "T", "version": "1" },
      "paths": {}
    }
    """;

    /// <summary>
    /// Tests that the ScanRunner.RunAsync method returns the correct exit code based on the failOn severity level and the findings produced by a rule with a fixed severity.
    /// </summary>
    /// <param name="failOn"></param>
    /// <param name="expected"></param>
    /// <returns></returns>

    [Theory]
    [InlineData(Severity.High, ExitCodes.Success)]
    [InlineData(Severity.Medium, ExitCodes.FindingsFound)]
    [InlineData(Severity.Low, ExitCodes.FindingsFound)]
    public async Task FailOn_decides_the_exit_code_for_a_medium_finding(Severity failOn, int expected)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(path, Spec);
            var runner = new ScanRunner(
                new Scanner([new FixedRule(Severity.Medium)]),
                new StringWriter(),
                new StringWriter());

            var code = await runner.RunAsync(path, failOn);

            Assert.Equal(expected, code);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Tests that the ScanReport.HasFindingsAtOrAbove method correctly compares findings by severity order.
    /// </summary>

    [Fact]
    public void HasFindingsAtOrAbove_compares_by_severity_order()
    {
        var report = new ScanReport("x.json",
            [new Finding("A", Severity.Low, "GET /a", "m")]);

        Assert.True(report.HasFindingsAtOrAbove(Severity.Low));
        Assert.True(report.HasFindingsAtOrAbove(Severity.Info));
        Assert.False(report.HasFindingsAtOrAbove(Severity.Medium));
    }
}