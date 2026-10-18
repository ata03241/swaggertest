using Microsoft.OpenApi;
using SwaggerTest.Core.Models;
using SwaggerTest.Core.Rules;

namespace SwaggerTest.Core;

public sealed class Scanner(IEnumerable<IRule> rules)
{
    public ScanReport Scan(OpenApiDocument document, string specPath)
    {
        var findings = rules
            .SelectMany(rule => rule.Evaluate(document))
            .OrderByDescending(f => f.Severity)
            .ThenBy(f => f.Endpoint)
            .ToList();

        return new ScanReport(specPath, findings);
    }
}