using Microsoft.OpenApi;

namespace SwaggerTest.Core.Spec;

public static class SpecLoader
{
    public static SpecLoadResult LoadFromText(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return SpecLoadResult.Fail("Spec is empty.");

        try
        {
            var result = OpenApiDocument.Parse(content, "json");

            var errors = result.Diagnostic?.Errors
                .Select(e => $"{e.Pointer}: {e.Message}")
                .ToList() ?? [];

            if (result.Document is null && errors.Count == 0)
                errors.Add("Spec could not be parsed.");

            return new SpecLoadResult(result.Document, errors);
        }
        catch (Exception ex)
        {
            return SpecLoadResult.Fail($"Spec could not be parsed: {ex.Message}");
        }
    }

    public static async Task<SpecLoadResult> LoadFromFileAsync(
        string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
            return SpecLoadResult.Fail($"File not found: {path}");

        var content = await File.ReadAllTextAsync(path, cancellationToken);
        return LoadFromText(content);
    }
}