using Microsoft.OpenApi;
using SwaggerTest.Core.Models;

namespace SwaggerTest.Core.Rules;

public sealed class NoAuthDeclaredRule : IRule
{
    public string Id => "NO_AUTH_DECLARED";

    private static readonly HashSet<string> WriteMethods = ["POST", "PUT", "PATCH", "DELETE"];

    // Paths that are normally public. A constant for now; a config file can replace it later.
    private static readonly string[] PublicPathHints =
        ["login", "signin", "register", "signup", "token", "health", "ping"];

    public IEnumerable<Finding> Evaluate(OpenApiDocument document)
    {
        if (document.Components?.SecuritySchemes is not { Count: > 0 })
        {
            yield return new Finding(
                Id,
                Severity.Medium,
                "(whole API)",
                "No authentication scheme is declared anywhere in this spec.",
                "Declare a security scheme and requirement, and enforce authentication in code. " +
                "If the API is meant to be public, you can ignore this.");
        }

        foreach (var (route, pathItem) in document.Paths)
        {
            if (pathItem.Operations is null) continue;
            if (LooksPublic(route)) continue;

            foreach (var (method, operation) in pathItem.Operations)
            {
                var verb = method.ToString().ToUpperInvariant();
                if (!WriteMethods.Contains(verb)) continue;

                // An operation's own security replaces the document level one.
                var effective = operation.Security ?? document.Security;

                if (IsProtected(effective)) continue;

                yield return new Finding(
                    Id,
                    Severity.High,
                    $"{verb} {route}",
                    $"{verb} {route} declares no authentication.",
                    "Declare a security requirement for this operation and enforce it in code. " +
                    "If it should be public, declare security: [] to say so.");
            }
        }
    }

    private static bool IsProtected(IList<OpenApiSecurityRequirement>? effective)
    {
        if (effective is { Count: 0 }) return true;

        // No requirement at all means open to anyone.
        if (effective is null) return false;

        // Every alternative must need a scheme. An empty requirement ({}) allows anonymous access.
        return effective.All(requirement => requirement.Count > 0);
    }

    private static bool LooksPublic(string route) =>
        PublicPathHints.Any(hint => route.Contains(hint, StringComparison.OrdinalIgnoreCase));
}