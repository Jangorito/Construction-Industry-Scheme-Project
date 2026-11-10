using CisCompliance.Core.Classification;
using CisCompliance.Core.Models;
using Xunit;

namespace CisCompliance.Tests.Classification;

public class CitationResolverTests
{
    private static readonly RuleDefinition KnownRule =
        new("R1", "Construction operations", "inclusion", "hmrc", "CIS340", "CIS340: Construction Industry Scheme guide", "https://example.invalid/cis340", "primary");

    [Fact]
    public void Resolve_KnownRuleId_ReturnsResolvedCitationWithSourceDetails()
    {
        var rulesById = new Dictionary<string, RuleDefinition> { [KnownRule.Id] = KnownRule };

        var citations = CitationResolver.Resolve(["R1"], rulesById);

        var citation = Assert.Single(citations);
        Assert.Equal("R1", citation.RuleId);
        Assert.True(citation.Resolved);
        Assert.Equal(KnownRule.SourceName, citation.SourceName);
        Assert.Equal(KnownRule.SourceTitle, citation.SourceTitle);
        Assert.Equal(KnownRule.SourceUrl, citation.SourceUrl);
        Assert.Equal(KnownRule.Verified, citation.Verified);
    }

    [Fact]
    public void Resolve_UnknownRuleId_ReturnsUnresolvedCitationWithoutThrowing()
    {
        // The exact scenario classify.py's --dry-run mode deliberately
        // exercised with a fake "R99": an id that isn't in the rules index
        // should come back visibly unresolved, not throw and not be
        // silently dropped.
        var rulesById = new Dictionary<string, RuleDefinition> { [KnownRule.Id] = KnownRule };

        var citations = CitationResolver.Resolve(["R99"], rulesById);

        var citation = Assert.Single(citations);
        Assert.Equal("R99", citation.RuleId);
        Assert.False(citation.Resolved);
        Assert.Null(citation.SourceName);
    }
}
