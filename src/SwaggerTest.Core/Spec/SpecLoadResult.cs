using Microsoft.OpenApi;

namespace SwaggerTest.Core.Spec;

/// <summary>
/// Represents the result of loading an OpenAPI specification, including the loaded document and any errors encountered during the loading process.
/// </summary>
/// <param name="Document"></param>
/// <param name="Errors"></param>

public sealed record SpecLoadResult(
    OpenApiDocument? Document,
    IReadOnlyList<string> Errors)
{
    public bool Success => Document is not null && Errors.Count == 0;

    public static SpecLoadResult Fail(string error) => new(null, [error]);
}