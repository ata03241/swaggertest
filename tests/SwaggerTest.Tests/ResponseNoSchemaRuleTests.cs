using SwaggerTest.Core.Models;
using SwaggerTest.Core.Rules;
using SwaggerTest.Core.Spec;

namespace SwaggerTest.Tests;

public class ResponseNoSchemaRuleTests
{
    private static List<Finding> Run(string specJson)
    {
        var load = SpecLoader.LoadFromText(specJson);
        Assert.True(load.Success, string.Join("; ", load.Errors));
        return new ResponseNoSchemaRule().Evaluate(load.Document!).ToList();
    }

    private static string Spec(string pathsJson, string componentsJson = "{}") => $$"""
    {
      "openapi": "3.0.1",
      "info": { "title": "T", "version": "1" },
      "paths": {{pathsJson}},
      "components": {{componentsJson}}
    }
    """;

    [Fact]
    public void Flags_a_200_with_no_content()
    {
        var findings = Run(Spec("""
        { "/x": { "get": { "responses": { "200": { "description": "ok" } } } } }
        """));

        var finding = Assert.Single(findings);
        Assert.Equal("RESPONSE_NO_SCHEMA", finding.RuleId);
        Assert.Equal(Severity.Info, finding.Severity);
        Assert.Equal("GET /x", finding.Endpoint);
    }

    [Fact]
    public void Flags_a_201_with_no_content()
    {
        var findings = Run(Spec("""
        { "/x": { "post": { "responses": { "201": { "description": "created" } } } } }
        """));

        Assert.Equal("POST /x", Assert.Single(findings).Endpoint);
    }

    [Fact]
    public void Flags_a_media_type_with_no_schema()
    {
        var findings = Run(Spec("""
        { "/x": { "get": { "responses": { "200": {
            "description": "ok",
            "content": { "application/json": {} } } } } } }
        """));

        Assert.Single(findings);
    }

    [Fact]
    public void Flags_a_bare_object_schema()
    {
        var findings = Run(Spec("""
        { "/x": { "get": { "responses": { "200": {
            "description": "ok",
            "content": { "application/json": { "schema": { "type": "object" } } } } } } } }
        """));

        Assert.Single(findings);
    }

    [Fact]
    public void Does_not_flag_a_schema_with_properties()
    {
        var findings = Run(Spec("""
        { "/x": { "get": { "responses": { "200": {
            "description": "ok",
            "content": { "application/json": { "schema": {
              "type": "object",
              "properties": { "id": { "type": "integer" } } } } } } } } } }
        """));

        Assert.Empty(findings);
    }

    [Fact]
    public void Does_not_flag_a_dictionary_schema()
    {
        var findings = Run(Spec("""
        { "/x": { "get": { "responses": { "200": {
            "description": "ok",
            "content": { "application/json": { "schema": {
              "type": "object",
              "additionalProperties": { "type": "string" } } } } } } } } }
        """));

        Assert.Empty(findings);
    }

    [Fact]
    public void Does_not_flag_a_ref_to_a_real_schema()
    {
        var findings = Run(Spec("""
        { "/x": { "get": { "responses": { "200": {
            "description": "ok",
            "content": { "application/json": { "schema": {
              "$ref": "#/components/schemas/Thing" } } } } } } } }
        """, """
        { "schemas": { "Thing": {
            "type": "object",
            "properties": { "id": { "type": "integer" } } } } }
        """));

        Assert.Empty(findings);
    }

    [Fact]
    public void Does_not_flag_204_which_has_no_body()
    {
        var findings = Run(Spec("""
        { "/x": { "delete": { "responses": { "204": { "description": "gone" } } } } }
        """));

        Assert.Empty(findings);
    }

    [Fact]
    public void Does_not_flag_error_responses()
    {
        var findings = Run(Spec("""
        { "/x": { "get": { "responses": { "404": { "description": "missing" } } } } }
        """));

        Assert.Empty(findings);
    }

    [Fact]
    public void Names_the_right_endpoint_when_only_one_is_bad()
    {
        var findings = Run(Spec("""
        {
          "/good": { "get": { "responses": { "200": {
            "description": "ok",
            "content": { "application/json": { "schema": {
              "type": "object",
              "properties": { "id": { "type": "integer" } } } } } } } } },
          "/bad": { "get": { "responses": { "200": { "description": "ok" } } } }
        }
        """));

        Assert.Equal("GET /bad", Assert.Single(findings).Endpoint);
    }
}