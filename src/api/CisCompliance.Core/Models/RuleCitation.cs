namespace CisCompliance.Core.Models;

// SourceName/SourceTitle/SourceUrl/Verified are nullable: an unresolved
// citation (a rule id the model cited that isn't found in the rules index)
// only ever has RuleId and Resolved=false. This should be structurally
// unreachable given the response schema's enum constraint, but is handled
// anyway rather than assumed impossible.
public sealed record RuleCitation(
    string RuleId,
    bool Resolved,
    string? SourceName,
    string? SourceTitle,
    string? SourceUrl,
    string? Verified);
