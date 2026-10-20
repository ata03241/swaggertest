using SwaggerTest.Core.Spec;

namespace SwaggerTest.Core;

public sealed class ScanRunner(Scanner scanner, TextWriter output, TextWriter error)
{
    public async Task<int> RunAsync(string specPath, CancellationToken cancellationToken = default)
    {
        var load = await SpecLoader.LoadFromFileAsync(specPath, cancellationToken);

        if (!load.Success)
        {
            foreach (var message in load.Errors)
                error.WriteLine($"error: {message}");
            return ExitCodes.ToolError;
        }

        var report = scanner.Scan(load.Document!, specPath);
        output.WriteLine(ReportSerializer.ToJson(report));

        return report.HasHighSeverity ? ExitCodes.FindingsFound : ExitCodes.Success;
    }
}