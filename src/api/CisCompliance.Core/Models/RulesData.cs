namespace CisCompliance.Core.Models;

// Summary:
// Everything the model needs to know about the rules, loaded from two rule files:
// - cis_rules_index.json: the machine readable rule registry 
// - cis_rules.md: the human readable rules document

// - RulesText: the full text of the rules document (cis_rules.md)
// - RuleIds: the full list of ids the model is allowed to cite (used to restrict the model's id output)
// - RulesById: a dictionary mapping rule ids to the RuleDefinition for that rule

public sealed record RulesData(
    string RulesText,
    IReadOnlyList<string> RuleIds,
    IReadOnlyDictionary<string, RuleDefinition> RulesById);
