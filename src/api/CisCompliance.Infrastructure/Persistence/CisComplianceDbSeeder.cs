using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Classification;
using CisCompliance.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CisCompliance.Infrastructure.Persistence;

// Runtime "if empty, insert" seeding rather than migration-baked HasData:
// HasData for owned-collection citation data would need manually assigned
// shadow keys, fragile and unreadable for 8 invoices worth of nested data.
// Resolves each seed line's rule ids against the real cis_rules_index.json
// via the same CitationResolver a live classification uses, so seed
// citations carry real source data rather than hand-guessed strings. Never
// calls the OpenAI client, a fresh checkout is fully populated with zero
// API key required for anything except the live Upload flow.
public static class CisComplianceDbSeeder
{
    public static async Task SeedIfEmptyAsync(
        CisComplianceDbContext db, IRulesRepository rulesRepository, CancellationToken cancellationToken = default)
    {
        if (await db.Invoices.AnyAsync(cancellationToken))
        {
            return;
        }

        var rules = await rulesRepository.LoadAsync(cancellationToken);

        foreach (var seedInvoice in InvoiceSeedData.Invoices)
        {
            var entity = new InvoiceEntity
            {
                InvoiceId = seedInvoice.InvoiceId,
                Supplier = seedInvoice.Supplier,
                Customer = seedInvoice.Customer,
                Date = seedInvoice.Date,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                LineItems = seedInvoice.Lines.Select((line, index) => new InvoiceLineEntity
                {
                    LineId = $"{seedInvoice.InvoiceId}-L{index + 1}",
                    Description = line.Description,
                    AmountGbp = line.AmountGbp,
                    PredictedLabel = line.Label,
                    PredictedConfidence = line.Confidence,
                    Reasoning = line.Reasoning,
                    EffectiveLabel = line.Label,
                    IsOverridden = false,
                    Citations = CitationResolver.Resolve(line.RuleIds, rules.RulesById).Select(c => new RuleCitationEntity
                    {
                        RuleId = c.RuleId,
                        Resolved = c.Resolved,
                        SourceName = c.SourceName,
                        SourceTitle = c.SourceTitle,
                        SourceUrl = c.SourceUrl,
                        Verified = c.Verified,
                    }).ToList(),
                }).ToList(),
            };

            db.Invoices.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
