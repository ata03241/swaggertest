namespace SwaggerTest.Core.Models;

public sealed record ScanReport(string SpecPath, IReadOnlyList<Finding> Findings)
{
    public int Total => Findings.Count;
    public bool HasHighSeverity => Findings.Any(f => f.Severity == Severity.High);

    public bool HasFindingsAtOrAbove(Severity threshold) =>
        Findings.Any(f => f.Severity >= threshold);
}