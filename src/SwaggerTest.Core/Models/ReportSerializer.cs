using System.Text.Json;
using System.Text.Json.Serialization;
using SwaggerTest.Core.Models;

namespace SwaggerTest.Core;

public static class ReportSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
         WriteIndented = true,
         Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string ToJson(ScanReport report) =>
        JsonSerializer.Serialize(report, Options);
}