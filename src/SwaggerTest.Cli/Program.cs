using System.CommandLine;
using SwaggerTest.Core;
using SwaggerTest.Core.Rules;

var specArgument = new Argument<FileInfo>("spec")
{
    Description = "Path to an OpenAPI spec file (JSON)."
};

var scanCommand = new Command("scan", "Scan an OpenAPI spec for common API problems.");
scanCommand.Arguments.Add(specArgument);
scanCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var spec = parseResult.GetValue(specArgument)!;
    var scanner = new Scanner([new SensitiveFieldRule()]);
    var runner = new ScanRunner(scanner, Console.Out, Console.Error);
    return await runner.RunAsync(spec.FullName, cancellationToken);
});

var root = new RootCommand("swaggertest: checks REST APIs for common problems");
root.Subcommands.Add(scanCommand);

var parseResult = root.Parse(args);
if (parseResult.Errors.Count > 0)
{
    foreach (var error in parseResult.Errors)
        Console.Error.WriteLine($"error: {error.Message}");
    return ExitCodes.ToolError;
}

return await parseResult.InvokeAsync();