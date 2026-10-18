using Microsoft.OpenApi;
using SwaggerTest.Core.Models;

namespace SwaggerTest.Core.Rules;

/// <summary>
/// Represents a rule that checks for sensitive fields in the response models of an OpenAPI document.
/// </summary>

public sealed class SensitiveFieldRule : IRule
{
    public string Id => "SENSITIVE_FIELD";

    private const int MaxDepth = 8;

    // Compared after normalising: lowercase, no '_' or '-'.
    private static readonly HashSet<string> SensitiveNames = new()
    {
        "password", "passwordhash", "hashedpassword", "passwordsalt",
        "secret", "clientsecret", "apikey", "privatekey",
        "accesstoken", "refreshtoken", "token",
        "ssn", "socialsecuritynumber", "creditcardnumber", "cardnumber", "cvv"
    };

    // It traverses the schema of each response and identifies any properties that match a predefined list of sensitive names. If such properties are found, it generates findings indicating the presence of sensitive data in the API responses.

    public IEnumerable<Finding> Evaluate(OpenApiDocument document)
    {
        foreach (var (route, pathItem) in document.Paths)
        {
            if (pathItem.Operations is null) continue;

            foreach (var (method, operation) in pathItem.Operations)
            {
                if (operation.Responses is null) continue;

                foreach (var (status, response) in operation.Responses)
                {
                    if (response.Content is null) continue;

                    foreach (var (_, media) in response.Content)
                    {
                        foreach (var hit in Walk(media.Schema, "$", 0))
                        {
                            yield return new Finding(
                                Id,
                                Severity.High,
                                $"{method.ToString().ToUpperInvariant()} {route}",
                                $"Response {status} exposes sensitive field '{hit}'.",
                                "Remove it from the response model, or map to a DTO that excludes it.");
                        }
                    }
                }
            }
        }
    }

    // Recursively traverses the schema to find sensitive fields, keeping track of the current path and depth to avoid excessive recursion.

    private static IEnumerable<string> Walk(IOpenApiSchema? schema, string path, int depth)
    {
        if (schema is null || depth > MaxDepth) yield break;

        if (schema.Properties is not null)
        {
            foreach (var (name, child) in schema.Properties)
            {
                var childPath = $"{path}.{name}";
                if (IsSensitive(name)) yield return childPath;

                foreach (var hit in Walk(child, childPath, depth + 1))
                    yield return hit;
            }
        }

        foreach (var hit in Walk(schema.Items, $"{path}[]", depth + 1))
            yield return hit;

        foreach (var inner in Combined(schema))
            foreach (var hit in Walk(inner, path, depth + 1))
                yield return hit;
    }

    // Combines the schemas from allOf, oneOf, and anyOf into a single enumerable for easier traversal.
    //Allof: The `allOf` keyword in OpenAPI allows you to combine multiple schemas into one. 
    //Oneof: The `oneOf` keyword in OpenAPI allows you to specify that a value must match exactly one of the given schemas.
    //Anyof: The `anyOf` keyword in OpenAPI allows you to specify that a value must match at least one of the given schemas.

    private static IEnumerable<IOpenApiSchema> Combined(IOpenApiSchema schema) =>
        (schema.AllOf ?? []).Concat(schema.OneOf ?? []).Concat(schema.AnyOf ?? []);


    // Checks if a given field name is considered sensitive by normalizing it and checking against the predefined list of sensitive names.
    private static bool IsSensitive(string name) =>
        SensitiveNames.Contains(name.Replace("_", "").Replace("-", "").ToLowerInvariant());
}