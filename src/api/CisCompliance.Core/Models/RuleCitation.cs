namespace CisCompliance.Core.Models;

// Summary:
// A record that represents one rule the AI cited for a line item. Contains information such as:
// - the rule ID
// - the real HMRC source name, title, and URL  
// - etc.
// The AI only provides the ID, the CitationResolver service looks up the rest which is how 
//  hallucinated citations can be caught through the Resolved bool. 
// Some properties are nullable because an unresolved rule citation won't have a source, etc.
public sealed record RuleCitation(
    string RuleId,
    bool Resolved,
    string? SourceName,
    string? SourceTitle,
    string? SourceUrl,
    string? Verified);
