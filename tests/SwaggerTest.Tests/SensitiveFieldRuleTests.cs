using SwaggerTest.Core.Models;
using SwaggerTest.Core.Rules;
using SwaggerTest.Core.Spec;

namespace SwaggerTest.Tests;

/// <summary>
/// Contains unit tests for the SensitiveFieldRule, which checks for sensitive fields in OpenAPI specifications.
/// </summary>

public class SensitiveFieldRuleTests
{
    private static List<Finding> Run(string specJson)
    {
        var load = SpecLoader.LoadFromText(specJson);
        Assert.True(load.Success, string.Join("; ", load.Errors));
        return new SensitiveFieldRule().Evaluate(load.Document!).ToList();
    }

    [Fact]
    public void Flags_sensitive_field_inside_referenced_array_item()
    {
        var findings = Run("""
        {
          "openapi": "3.0.1",
          "info": { "title": "T", "version": "1" },
          "paths": { "/users": { "get": { "responses": { "200": {
            "description": "ok",
            "content": { "application/json": { "schema": {
              "type": "array",
              "items": { "$ref": "#/components/schemas/User" } } } } } } } } },
          "components": { "schemas": { "User": {
            "type": "object",
            "properties": {
              "id": { "type": "integer" },
              "email": { "type": "string" },
              "passwordHash": { "type": "string" } } } } }
        }
        """);

        var finding = Assert.Single(findings);
        Assert.Equal("SENSITIVE_FIELD", finding.RuleId);
        Assert.Equal(Severity.High, finding.Severity);
        Assert.Equal("GET /users", finding.Endpoint);
        Assert.Contains("passwordHash", finding.Message);
    }

    [Fact]
    public void Ignores_password_in_request_body()
    {
        var findings = Run("""
        {
          "openapi": "3.0.1",
          "info": { "title": "T", "version": "1" },
          "paths": { "/login": { "post": {
            "requestBody": { "content": { "application/json": { "schema": {
              "type": "object",
              "properties": { "password": { "type": "string" } } } } } },
            "responses": { "200": { "description": "ok" } } } } }
        }
        """);

        Assert.Empty(findings);
    }

    [Fact]
    public void Returns_nothing_for_a_clean_response()
    {
        var findings = Run("""
        {
          "openapi": "3.0.1",
          "info": { "title": "T", "version": "1" },
          "paths": { "/ping": { "get": { "responses": { "200": {
            "description": "ok",
            "content": { "application/json": { "schema": {
              "type": "object",
              "properties": { "status": { "type": "string" } } } } } } } } } }
        }
        """);

        Assert.Empty(findings);
    }
}