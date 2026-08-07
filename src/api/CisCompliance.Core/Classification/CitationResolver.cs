using CisCompliance.Core.Models;

namespace CisCompliance.Core.Classification;

// Ported from resolve_citations() in classify.py. Turns a list of rule ids
// the model cited into the real HMRC source info behind each one, by
// looking it up in the rules index rather than trusting anything the model
// said about where the rule came from. An id that doesn't resolve (should
// be structurally unreachable given the schema's enum constraint, but
// checked anyway in case that constraint is ever relaxed) is kept in the
// output with Resolved=false so it stays visible, not dropped.
public static class CitationResolver
{
    public static IReadOnlyList<RuleCitation> Resolve(
        IEnumerable<string> ruleIds, IReadOnlyDictionary<string, RuleDefinition> rulesById)
    {
        var citations = new List<RuleCitation>();
        foreach (var ruleId in ruleIds)
        {
            citations.Add(rulesById.TryGetValue(ruleId, out var rule)
                ? new RuleCitation(ruleId, true, rule.SourceName, rule.SourceTitle, rule.SourceUrl, rule.Verified)
                : new RuleCitation(ruleId, false, null, null, null, null));
        }

        return citations;
    }
}
