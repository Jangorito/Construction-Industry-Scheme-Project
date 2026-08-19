namespace CisCompliance.Infrastructure.Persistence.Entities;

// Owned type, no DbSet: a citation has no identity of its own, is never
// queried or updated independently of its line, and is always
// loaded/saved/deleted along with its parent InvoiceLineEntity.
public sealed class RuleCitationEntity
{
    public string RuleId { get; set; } = string.Empty;
    public bool Resolved { get; set; }
    public string? SourceName { get; set; }
    public string? SourceTitle { get; set; }
    public string? SourceUrl { get; set; }
    public string? Verified { get; set; }
}
