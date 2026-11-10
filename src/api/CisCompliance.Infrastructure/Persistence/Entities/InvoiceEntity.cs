namespace CisCompliance.Infrastructure.Persistence.Entities;

public sealed class InvoiceEntity
{
    // The app-level id itself (e.g. "INV-2026-0001") is the primary key,
    // no separate surrogate key, there's no need for one and it would just
    // be a second id system to keep in sync with the first.
    public string InvoiceId { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public List<InvoiceLineEntity> LineItems { get; set; } = new();
}
