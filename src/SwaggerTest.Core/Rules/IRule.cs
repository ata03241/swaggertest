using Microsoft.OpenApi;
using SwaggerTest.Core.Models;

namespace SwaggerTest.Core.Rules;

public interface IRule
{
    string Id { get; }
    IEnumerable<Finding> Evaluate(OpenApiDocument document);
}