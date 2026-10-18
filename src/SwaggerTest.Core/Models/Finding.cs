namespace SwaggerTest.Core.Models;

public sealed record Finding(
    string RuleId,
    Severity Severity,
    string Endpoint,
    string Message,
    string? FixHint = null);