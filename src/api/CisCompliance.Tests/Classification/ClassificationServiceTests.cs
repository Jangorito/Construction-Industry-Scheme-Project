using CisCompliance.Core.Classification;
using CisCompliance.Core.Exceptions;
using CisCompliance.Core.Models;
using CisCompliance.Tests.TestDoubles;
using Xunit;

namespace CisCompliance.Tests.Classification;

public class ClassificationServiceTests
{
    private static readonly RuleDefinition R1 =
        new("R1", "Construction operations", "inclusion", "hmrc", "CIS340", "CIS340 guide", "https://example.invalid/cis340", "primary");

    private static InvoiceClassificationRequest OneLineRequest() => new(
        "INV-TEST-0001", "Some Supplier Ltd", "Some Customer Ltd", new DateOnly(2026, 1, 15),
        [new InvoiceLineItemRequest("L1", "Groundworks and foundations", 1000m)]);

    [Fact]
    public async Task ClassifyAsync_UnresolvableRuleIdInModelResponse_ReturnsUnresolvedCitationWithoutThrowing()
    {
        // Full-pipeline counterpart to CitationResolverTests: proves an
        // unresolvable rule id survives the whole round trip (deserialize,
        // map to enums, resolve citations) without the service throwing,
        // mirroring classify.py's dry-run "R99" check end to end.
        var rules = new RulesData("rules text", ["R1"], new Dictionary<string, RuleDefinition> { [R1.Id] = R1 });
        var rawResponse = """
            {
              "line_items": [
                { "line_id": "L1", "reasoning": "test reasoning", "rule_ids": ["R99"], "confidence": "high", "label": "Construction" }
              ]
            }
            """;

        var service = new ClassificationService(new FakeRulesRepository(rules), new FakeClassificationModelClient(rawResponse));

        var result = await service.ClassifyAsync(OneLineRequest());

        var line = Assert.Single(result.LineItems);
        var citation = Assert.Single(line.Citations);
        Assert.Equal("R99", citation.RuleId);
        Assert.False(citation.Resolved);
    }

    [Fact]
    public async Task ClassifyAsync_ModelOmitsRequestedLineItem_ThrowsClassificationModelException()
    {
        var rules = new RulesData("rules text", ["R1"], new Dictionary<string, RuleDefinition> { [R1.Id] = R1 });
        var rawResponse = """{ "line_items": [] }""";

        var service = new ClassificationService(new FakeRulesRepository(rules), new FakeClassificationModelClient(rawResponse));

        await Assert.ThrowsAsync<ClassificationModelException>(() => service.ClassifyAsync(OneLineRequest()));
    }

    [Fact]
    public async Task ClassifyAsync_ValidResponse_MapsLabelAndConfidenceCorrectly()
    {
        var rules = new RulesData("rules text", ["R1"], new Dictionary<string, RuleDefinition> { [R1.Id] = R1 });
        var rawResponse = """
            {
              "line_items": [
                { "line_id": "L1", "reasoning": "clear construction work", "rule_ids": ["R1"], "confidence": "low", "label": "Construction" }
              ]
            }
            """;

        var service = new ClassificationService(new FakeRulesRepository(rules), new FakeClassificationModelClient(rawResponse));

        var result = await service.ClassifyAsync(OneLineRequest());

        var line = Assert.Single(result.LineItems);
        Assert.Equal(ClassificationLabel.Construction, line.Label);
        Assert.Equal(ConfidenceLevel.Low, line.Confidence);
        Assert.Equal("clear construction work", line.Reasoning);
    }
}
