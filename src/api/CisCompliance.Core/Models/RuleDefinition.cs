namespace CisCompliance.Core.Models;

// Mirrors one entry from cis_rules_index.json (the machine readable rule
// registry). This is what a cited rule id resolves to.

// Summary:
// The schema for a single HMRC rule mirroring the JSON registry (cis_rules_index.json) 
//  (loaded at startup). 
// A rule id is produced by the model, the CitationResolver service finds the 
//  correspoding RuleDefinition and uses those properties to populate a RuleCitation 
//  for the line item. 
public sealed record RuleDefinition(
    string Id,
    string Title,
    string Category,
    string SourceType,
    string SourceName,
    string SourceTitle,
    string? SourceUrl,
    string Verified);
