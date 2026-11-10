using CisCompliance.Core.Exceptions;
using CisCompliance.Core.Extraction;
using CisCompliance.Tests.TestDoubles;
using Xunit;

namespace CisCompliance.Tests.Extraction;

public class InvoiceExtractionServiceTests
{
    private static InvoiceExtractionService ServiceWithResponse(string rawJson) =>
        new(new FakeExtractionModelClient(rawJson));

    [Fact]
    public async Task ExtractAsync_ValidResponse_MapsEveryFieldAndSynthesizesLineIds()
    {
        var raw = """
            {
              "invoice_number": "INV-2026-9001",
              "supplier": "Ashcroft Building Services Ltd",
              "customer": "Oakfield Property Group",
              "date": "2026-07-30",
              "line_items": [
                { "description": "Erection of scaffolding", "amount_gbp": 1967.65 },
                { "description": "Preparation and laying of foundations", "amount_gbp": 12612.57 }
              ]
            }
            """;
        var service = ServiceWithResponse(raw);

        var result = await service.ExtractAsync([], "application/pdf", "invoice.pdf");

        Assert.Equal("INV-2026-9001", result.InvoiceId);
        Assert.Equal("Ashcroft Building Services Ltd", result.Supplier);
        Assert.Equal("Oakfield Property Group", result.Customer);
        Assert.Equal(new DateOnly(2026, 7, 30), result.Date);
        Assert.Equal(2, result.LineItems.Count);
        Assert.Equal("INV-2026-9001-L1", result.LineItems[0].LineId);
        Assert.Equal("INV-2026-9001-L2", result.LineItems[1].LineId);
        Assert.Equal(1967.65m, result.LineItems[0].AmountGbp);
    }

    [Fact]
    public async Task ExtractAsync_NullInvoiceNumber_GeneratesFallbackIdUsedForLineIdsToo()
    {
        var raw = """
            {
              "invoice_number": null,
              "supplier": "Some Supplier Ltd",
              "customer": null,
              "date": "2026-01-15",
              "line_items": [
                { "description": "Groundworks", "amount_gbp": 500.00 }
              ]
            }
            """;
        var service = ServiceWithResponse(raw);

        var result = await service.ExtractAsync([], "image/png", "invoice.png");

        Assert.StartsWith("EXT-", result.InvoiceId);
        Assert.Equal("Unknown customer", result.Customer);
        Assert.Equal(result.InvoiceId + "-L1", result.LineItems[0].LineId);
    }

    [Theory]
    [InlineData("not-a-date")]
    [InlineData(null)]
    public async Task ExtractAsync_UnparsableOrMissingDate_ThrowsInvoiceExtractionException(string? badDate)
    {
        var dateJson = badDate is null ? "null" : $"\"{badDate}\"";
        var raw = $$"""
            {
              "invoice_number": "INV-1",
              "supplier": "Some Supplier Ltd",
              "customer": "Some Customer Ltd",
              "date": {{dateJson}},
              "line_items": [ { "description": "Groundworks", "amount_gbp": 500.00 } ]
            }
            """;
        var service = ServiceWithResponse(raw);

        await Assert.ThrowsAsync<InvoiceExtractionException>(() => service.ExtractAsync([], "application/pdf", "invoice.pdf"));
    }

    [Fact]
    public async Task ExtractAsync_EmptyLineItems_ThrowsInvoiceExtractionException()
    {
        var raw = """
            {
              "invoice_number": "INV-1",
              "supplier": "Some Supplier Ltd",
              "customer": "Some Customer Ltd",
              "date": "2026-01-15",
              "line_items": []
            }
            """;
        var service = ServiceWithResponse(raw);

        await Assert.ThrowsAsync<InvoiceExtractionException>(() => service.ExtractAsync([], "application/pdf", "invoice.pdf"));
    }

    [Fact]
    public async Task ExtractAsync_NullCustomer_FallsBackToUnknownCustomer()
    {
        var raw = """
            {
              "invoice_number": "INV-1",
              "supplier": "Some Supplier Ltd",
              "customer": null,
              "date": "2026-01-15",
              "line_items": [ { "description": "Groundworks", "amount_gbp": 500.00 } ]
            }
            """;
        var service = ServiceWithResponse(raw);

        var result = await service.ExtractAsync([], "application/pdf", "invoice.pdf");

        Assert.Equal("Unknown customer", result.Customer);
    }
}
