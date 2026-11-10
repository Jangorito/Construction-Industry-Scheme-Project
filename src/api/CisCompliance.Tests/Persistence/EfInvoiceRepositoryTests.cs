using CisCompliance.Core.Models;
using CisCompliance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CisCompliance.Tests.Persistence;

// Uses EF Core's InMemory provider, no real SQL Server touched, same
// principle as the rest of this test suite (no real network/file/DB
// dependency in any test) — a fresh, isolated database per test via a
// unique Guid name.
public class EfInvoiceRepositoryTests
{
    private static CisComplianceDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<CisComplianceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CisComplianceDbContext(options);
    }

    private static InvoiceClassificationRequest OneLineRequest(string invoiceId = "INV-TEST-0001") => new(
        invoiceId, "Test Supplier Ltd", "Test Customer Ltd", new DateOnly(2026, 1, 15),
        [new InvoiceLineItemRequest("L1", "Groundworks and foundations", 1000m)]);

    private static InvoiceClassificationResult OneLineResult(string invoiceId = "INV-TEST-0001") => new(
        invoiceId,
        [
            new LineItemClassification(
                "L1", ClassificationLabel.Construction, ConfidenceLevel.High, "Test reasoning",
                [new RuleCitation("R1", true, "CIS340", "CIS340 guide", "https://example.invalid/cis340", "primary")]),
        ]);

    [Fact]
    public async Task SaveAsync_ThenGetById_RoundTripsAllFields()
    {
        using var db = NewContext();
        var repo = new EfInvoiceRepository(db);
        var request = OneLineRequest();
        var result = OneLineResult();

        await repo.SaveAsync(request, result);
        var stored = await repo.GetByIdAsync(request.InvoiceId);

        Assert.NotNull(stored);
        Assert.Equal(request.Supplier, stored!.Supplier);
        Assert.Equal(request.Customer, stored.Customer);
        Assert.Equal(request.Date, stored.Date);

        var line = Assert.Single(stored.LineItems);
        Assert.Equal("L1", line.LineId);
        Assert.Equal("Groundworks and foundations", line.Description);
        Assert.Equal(1000m, line.AmountGbp);
        Assert.Equal(ClassificationLabel.Construction, line.PredictedLabel);
        Assert.Equal(ClassificationLabel.Construction, line.EffectiveLabel);
        Assert.False(line.IsOverridden);
        var citation = Assert.Single(line.Citations);
        Assert.Equal("R1", citation.RuleId);
    }

    [Fact]
    public async Task OverrideLineAsync_ChangesEffectiveLabel_ButKeepsOriginalPrediction()
    {
        // The actual audit-trail guarantee this whole feature exists for:
        // a human correction must never erase what the AI originally said.
        using var db = NewContext();
        var repo = new EfInvoiceRepository(db);
        var request = OneLineRequest();
        await repo.SaveAsync(request, OneLineResult());

        var updated = await repo.OverrideLineAsync(request.InvoiceId, "L1", ClassificationLabel.Services, "Confirmed with supplier");

        Assert.NotNull(updated);
        var line = Assert.Single(updated!.LineItems);
        Assert.Equal(ClassificationLabel.Construction, line.PredictedLabel);
        Assert.Equal(ClassificationLabel.Services, line.EffectiveLabel);
        Assert.True(line.IsOverridden);
        Assert.Equal("Confirmed with supplier", line.OverrideReason);
        Assert.NotNull(line.OverriddenAtUtc);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        using var db = NewContext();
        var repo = new EfInvoiceRepository(db);

        Assert.Null(await repo.GetByIdAsync("does-not-exist"));
    }

    [Fact]
    public async Task OverrideLineAsync_UnknownInvoiceOrLine_ReturnsNull()
    {
        using var db = NewContext();
        var repo = new EfInvoiceRepository(db);
        var request = OneLineRequest();
        await repo.SaveAsync(request, OneLineResult());

        Assert.Null(await repo.OverrideLineAsync("does-not-exist", "L1", ClassificationLabel.Services, "reason"));
        Assert.Null(await repo.OverrideLineAsync(request.InvoiceId, "does-not-exist", ClassificationLabel.Services, "reason"));
    }

    [Fact]
    public async Task SaveAsync_CalledTwiceForSameInvoiceId_ReplacesPreviousLineItems()
    {
        // Documents the chosen upsert-by-replace behaviour: re-classifying
        // an already-saved invoice discards its prior line items (and any
        // override on them), rather than throwing on the duplicate key.
        using var db = NewContext();
        var repo = new EfInvoiceRepository(db);
        var request = OneLineRequest();
        await repo.SaveAsync(request, OneLineResult());
        await repo.OverrideLineAsync(request.InvoiceId, "L1", ClassificationLabel.Services, "reason");

        await repo.SaveAsync(request, OneLineResult());

        var stored = await repo.GetByIdAsync(request.InvoiceId);
        var line = Assert.Single(stored!.LineItems);
        Assert.False(line.IsOverridden);
        Assert.Equal(ClassificationLabel.Construction, line.EffectiveLabel);
    }
}
