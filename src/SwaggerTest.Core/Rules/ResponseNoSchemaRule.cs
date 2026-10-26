using Microsoft.OpenApi;
using SwaggerTest.Core.Models;

namespace SwaggerTest.Core.Rules;

public sealed class ResponseNoSchemaRule : IRule
{
    public string Id => "RESPONSE_NO_SCHEMA";

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
                    if (!ExpectsBody(status)) continue;
                    if (!HasNoUsableSchema(response)) continue;

                    yield return new Finding(
                        Id,
                        Severity.Info,
                        $"{method.ToString().ToUpperInvariant()} {route}",
                        $"Response {status} declares no schema, so SENSITIVE_FIELD can't check it.",
                        "Declare the response type (in ASP.NET: .Produces<T>()).");
                }
            }
        }
    }

    // Success responses only. 204 and 205 are not supposed to have a body.
    private static bool ExpectsBody(string status) =>
        status.StartsWith('2') && status is not ("204" or "205");

    private static bool HasNoUsableSchema(IOpenApiResponse response)
    {
        if (response.Content is null || response.Content.Count == 0)
            return true;

        return response.Content.Values.Any(media => media.Schema is null || IsBare(media.Schema));
    }

    // "Bare" means the schema says nothing about the shape of the data.
    private static bool IsBare(IOpenApiSchema schema)
    {
        var hasProperties = schema.Properties is { Count: > 0 };
        var hasItems = schema.Items is not null;
        var hasCombinators =
            schema.AllOf is { Count: > 0 } ||
            schema.OneOf is { Count: > 0 } ||
            schema.AnyOf is { Count: > 0 };
        var isMap = schema.AdditionalProperties is not null;
        var typeIsOpen = schema.Type is null || schema.Type == JsonSchemaType.Object;

        return typeIsOpen && !hasProperties && !hasItems && !hasCombinators && !isMap;
    }
}