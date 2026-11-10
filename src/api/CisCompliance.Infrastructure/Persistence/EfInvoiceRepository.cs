using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Models;
using CisCompliance.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CisCompliance.Infrastructure.Persistence;

public sealed class EfInvoiceRepository : IInvoiceRepository
{
    private readonly CisComplianceDbContext _db;

    public EfInvoiceRepository(CisComplianceDbContext db)
    {
        _db = db;
    }

    // Upsert-by-replace: re-classifying an invoice that's already saved
    // deletes its old line items and rebuilds fresh ones from the new
    // result, rather than throwing on the duplicate key. This is a real,
    // deliberate trade-off, not an oversight: it means any prior human
    // override on that invoice is discarded, since the rebuilt lines start
    // IsOverridden=false again. Nothing in the current UI actually
    // re-classifies an already-saved invoice (the upload pool only offers
    // each invoice once), so this is a documented edge case, not a live bug.
    public async Task SaveAsync(
        InvoiceClassificationRequest request, InvoiceClassificationResult result, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, cancellationToken);
        if (existing is not null)
        {
            _db.Invoices.Remove(existing);
        }

        var lineItemsByLineId = result.LineItems.ToDictionary(li => li.LineId);

        var entity = new InvoiceEntity
        {
            InvoiceId = request.InvoiceId,
            Supplier = request.Supplier,
            Customer = request.Customer,
            Date = request.Date,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            LineItems = request.LineItems.Select(requested =>
            {
                var classification = lineItemsByLineId[requested.LineId];
                return new InvoiceLineEntity
                {
                    LineId = requested.LineId,
                    Description = requested.Description,
                    AmountGbp = requested.AmountGbp,
                    PredictedLabel = classification.Label,
                    PredictedConfidence = classification.Confidence,
                    Reasoning = classification.Reasoning,
                    EffectiveLabel = classification.Label,
                    IsOverridden = false,
                    Citations = classification.Citations.Select(ToEntity).ToList(),
                };
            }).ToList(),
        };

        _db.Invoices.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoredInvoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _db.Invoices
            .Include(i => i.LineItems).ThenInclude(l => l.Citations)
            .AsNoTracking()
            .OrderByDescending(i => i.Date)
            .ToListAsync(cancellationToken);

        return entities.Select(ToModel).ToList();
    }

    public async Task<StoredInvoice?> GetByIdAsync(string invoiceId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Invoices
            .Include(i => i.LineItems).ThenInclude(l => l.Citations)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);

        return entity is null ? null : ToModel(entity);
    }

    public async Task<StoredInvoice?> OverrideLineAsync(
        string invoiceId, string lineId, ClassificationLabel newLabel, string reason, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Invoices
            .Include(i => i.LineItems).ThenInclude(l => l.Citations)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);
        if (entity is null) return null;

        var line = entity.LineItems.FirstOrDefault(l => l.LineId == lineId);
        if (line is null) return null;

        line.EffectiveLabel = newLabel;
        line.IsOverridden = true;
        line.OverrideReason = reason;
        line.OverriddenAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return ToModel(entity);
    }

    private static StoredInvoice ToModel(InvoiceEntity entity) => new(
        entity.InvoiceId,
        entity.Supplier,
        entity.Customer,
        entity.Date,
        entity.CreatedAtUtc,
        entity.LineItems.OrderBy(l => l.Id).Select(ToModel).ToList());

    private static StoredLineItemClassification ToModel(InvoiceLineEntity entity) => new(
        entity.LineId,
        entity.Description,
        entity.AmountGbp,
        entity.PredictedLabel,
        entity.PredictedConfidence,
        entity.Reasoning,
        entity.Citations.Select(ToModel).ToList(),
        entity.EffectiveLabel,
        entity.IsOverridden,
        entity.OverrideReason,
        entity.OverriddenAtUtc);

    private static RuleCitation ToModel(RuleCitationEntity entity) => new(
        entity.RuleId, entity.Resolved, entity.SourceName, entity.SourceTitle, entity.SourceUrl, entity.Verified);

    private static RuleCitationEntity ToEntity(RuleCitation citation) => new()
    {
        RuleId = citation.RuleId,
        Resolved = citation.Resolved,
        SourceName = citation.SourceName,
        SourceTitle = citation.SourceTitle,
        SourceUrl = citation.SourceUrl,
        Verified = citation.Verified,
    };
}
