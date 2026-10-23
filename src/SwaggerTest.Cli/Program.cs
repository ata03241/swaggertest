using System.CommandLine;
using SwaggerTest.Core;
using SwaggerTest.Core.Rules;
using SwaggerTest.Core.Models;

var specArgument = new Argument<FileInfo>("spec")
{
    Description = "Path to an OpenAPI spec file (JSON)."
};

var failOnOption  = new Option<Severity>("--fail-on")
{
    Description = "Fail if any findings are at or above this severity level.",
    DefaultValueFactory = _ => Severity.High
};

var scanCommand = new Command("scan", "Scan an OpenAPI spec for common API problems.");
scanCommand.Arguments.Add(specArgument);
scanCommand.Options.Add(failOnOption);
scanCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var spec = parseResult.GetValue(specArgument)!;
    var failOn = parseResult.GetValue(failOnOption);
    var scanner = new Scanner([new SensitiveFieldRule()]);
    var runner = new ScanRunner(scanner, Console.Out, Console.Error);
    return await runner.RunAsync(spec.FullName, failOn, cancellationToken);
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