namespace CisCompliance.Core.Models;

// Mirrors one entry from cis_rules_index.json (the machine readable rule
// registry). This is what a cited rule id resolves to.
public sealed record RuleDefinition(
    string Id,
    string Title,
    string Category,
    string SourceType,
    string SourceName,
    string SourceTitle,
    string? SourceUrl,
    string Verified);
