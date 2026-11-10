using SwaggerTest.Core.Models;
using SwaggerTest.Core.Rules;
using SwaggerTest.Core.Spec;

namespace SwaggerTest.Tests;

public class NoAuthDeclaredRuleTests
{
    private const string BearerScheme = """
        "components": { "securitySchemes": { "bearer": { "type": "http", "scheme": "bearer" } } },
        """;

    private const string GlobalAuth = """
        "security": [ { "bearer": [] } ],
        """;

    private static List<Finding> Run(string pathsJson, string extraJson = "")
    {
        var spec = $$"""
        {
          "openapi": "3.0.1",
          "info": { "title": "T", "version": "1" },
          {{extraJson}}
          "paths": {{pathsJson}}
        }
        """;

        var load = SpecLoader.LoadFromText(spec);
        Assert.True(load.Success, string.Join("; ", load.Errors));
        return new NoAuthDeclaredRule().Evaluate(load.Document!).ToList();
    }

    [Fact]
    public void Flags_the_whole_api_when_no_security_scheme_exists()
    {
        var findings = Run("""
        { "/x": { "get": { "responses": { "200": { "description": "ok" } } } } }
        """);

        var finding = Assert.Single(findings);
        Assert.Equal("NO_AUTH_DECLARED", finding.RuleId);
        Assert.Equal(Severity.Medium, finding.Severity);
        Assert.Equal("(whole API)", finding.Endpoint);
    }

    [Fact]
    public void Flags_a_post_with_no_security()
    {
        var findings = Run("""
        { "/x": { "post": { "responses": { "201": { "description": "created" } } } } }
        """, BearerScheme);

        var finding = Assert.Single(findings);
        Assert.Equal(Severity.High, finding.Severity);
        Assert.Equal("POST /x", finding.Endpoint);
    }

    [Fact]
    public void Flags_a_delete_with_no_security()
    {
        var findings = Run("""
        { "/x": { "delete": { "responses": { "204": { "description": "gone" } } } } }
        """, BearerScheme);

        Assert.Equal("DELETE /x", Assert.Single(findings).Endpoint);
    }

    [Fact]
    public void Does_not_flag_a_post_covered_by_global_security()
    {
        var findings = Run("""
        { "/x": { "post": { "responses": { "201": { "description": "created" } } } } }
        """, BearerScheme + GlobalAuth);

        Assert.Empty(findings);
    }

    [Fact]
    public void Does_not_flag_a_post_with_its_own_security_requirement()
    {
        var findings = Run("""
        { "/x": { "post": {
            "security": [ { "bearer": [] } ],
            "responses": { "201": { "description": "created" } } } } }
        """, BearerScheme);

        Assert.Empty(findings);
    }

    [Fact]
    public void Treats_an_explicit_empty_security_list_as_public_on_purpose()
    {
        var findings = Run("""
        { "/x": { "post": {
            "security": [],
            "responses": { "201": { "description": "created" } } } } }
        """, BearerScheme + GlobalAuth);

        Assert.Empty(findings);
    }

    [Fact]
    public void Flags_a_post_that_also_allows_anonymous_access()
    {
        var findings = Run("""
        { "/x": { "post": {
            "security": [ { "bearer": [] }, {} ],
            "responses": { "201": { "description": "created" } } } } }
        """, BearerScheme);

        Assert.Equal("POST /x", Assert.Single(findings).Endpoint);
    }

    [Fact]
    public void Skips_paths_that_are_public_by_nature()
    {
        var findings = Run("""
        { "/auth/login": { "post": { "responses": { "200": { "description": "ok" } } } } }
        """, BearerScheme);

        Assert.Empty(findings);
    }

    [Fact]
    public void Does_not_flag_a_get_individually()
    {
        var findings = Run("""
        { "/x": { "get": { "responses": { "200": { "description": "ok" } } } } }
        """, BearerScheme);

        Assert.Empty(findings);
    }
}