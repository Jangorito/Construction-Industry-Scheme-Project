using System.Text.Json;
using CisCompliance.Core.Classification;
using Xunit;

namespace CisCompliance.Tests.Classification;

// This is the test that guards the single most important correctness
// constraint in the whole port: the model fills in JSON fields in the order
// the schema declares them, and never revises an earlier field once later
// ones are written. An earlier version of classify.py had "label" before
// "reasoning" and it measurably regressed accuracy, see
// ClassificationSchemaBuilder's class comment. If a future change
// accidentally reorders the properties below, this test should fail.
public class ClassificationSchemaBuilderTests
{
    [Fact]
    public void Build_PreservesLoadBearingPropertyOrder()
    {
        var schemaJson = ClassificationSchemaBuilder.Build(["R1", "R2"]);

        // JsonDocument, unlike a Dictionary, preserves the original
        // document's property order, which is what needs asserting here.
        using var document = JsonDocument.Parse(schemaJson);
        var lineItemProperties = document.RootElement
            .GetProperty("properties")
            .GetProperty("line_items")
            .GetProperty("items")
            .GetProperty("properties");

        var actualOrder = lineItemProperties.EnumerateObject().Select(p => p.Name).ToArray();

        Assert.Equal(["line_id", "reasoning", "rule_ids", "confidence", "label"], actualOrder);
    }

    [Fact]
    public void Build_RuleIdsEnum_ContainsExactlyTheSuppliedIds()
    {
        var schemaJson = ClassificationSchemaBuilder.Build(["R1", "R11a", "R13"]);

        using var document = JsonDocument.Parse(schemaJson);
        var ruleIdsEnum = document.RootElement
            .GetProperty("properties")
            .GetProperty("line_items")
            .GetProperty("items")
            .GetProperty("properties")
            .GetProperty("rule_ids")
            .GetProperty("items")
            .GetProperty("enum");

        var actualIds = ruleIdsEnum.EnumerateArray().Select(v => v.GetString()).ToArray();

        Assert.Equal(["R1", "R11a", "R13"], actualIds);
    }

    [Fact]
    public void Build_UsesStrictAdditionalPropertiesFalse()
    {
        // Not optional: additionalProperties:false at both levels is what
        // makes the schema strict-mode compatible, which is what stops the
        // model from returning fields outside the constrained shape.
        var schemaJson = ClassificationSchemaBuilder.Build(["R1"]);

        using var document = JsonDocument.Parse(schemaJson);
        Assert.False(document.RootElement.GetProperty("additionalProperties").GetBoolean());

        var lineItemSchema = document.RootElement
            .GetProperty("properties")
            .GetProperty("line_items")
            .GetProperty("items");
        Assert.False(lineItemSchema.GetProperty("additionalProperties").GetBoolean());
    }
}
