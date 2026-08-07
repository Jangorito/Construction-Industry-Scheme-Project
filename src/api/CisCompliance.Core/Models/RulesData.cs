namespace CisCompliance.Core.Models;

// What IRulesRepository hands back: the rules doc text (folded into the
// system prompt), the full list of ids the model is allowed to cite (used to
// build the response schema's enum constraint), and a lookup from id to the
// full rule definition (used to resolve a citation after the model answers).
public sealed record RulesData(
    string RulesText,
    IReadOnlyList<string> RuleIds,
    IReadOnlyDictionary<string, RuleDefinition> RulesById);
