using System.Globalization;
using System.Text.Json;
using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Exceptions;
using CisCompliance.Core.Models;

namespace CisCompliance.Core.Extraction;

// Orchestrates a single extraction: build prompt/schema, call the model,
// deserialize, apply fallback rules, return the same InvoiceClassificationRequest
// shape ClassifyController already accepts.
public sealed class InvoiceExtractionService : IInvoiceExtractionService
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly IExtractionModelClient _modelClient;

    public InvoiceExtractionService(IExtractionModelClient modelClient)
    {
        _modelClient = modelClient;
    }

    public async Task<InvoiceClassificationRequest> ExtractAsync(
        byte[] fileBytes, string mediaType, string fileName, CancellationToken cancellationToken = default)
    {
        var schema = InvoiceExtractionSchemaBuilder.Build();

        var raw = await _modelClient.GetExtractionCompletionAsync(
            InvoiceExtractionPrompt.SystemPrompt, InvoiceExtractionPrompt.UserInstruction,
            fileBytes, mediaType, fileName, schema, InvoiceExtractionSchemaBuilder.SchemaName, cancellationToken);

        ExtractionResponseDto? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<ExtractionResponseDto>(raw);
        }
        catch (JsonException ex)
        {
            throw new InvoiceExtractionException(
                "The model returned a response that did not match the expected schema.", ex);
        }

        if (parsed is null)
        {
            throw new InvoiceExtractionException("The model returned an empty response.");
        }

        if (parsed.LineItems.Count == 0)
        {
            throw new InvoiceExtractionException("The document did not contain any recognizable invoice line items.");
        }

        // Date has real downstream weight (sort order, dashboard
        // aggregates), unlike invoice number/customer, which are
        // cosmetically wrong at worst if missing. A silently-fabricated
        // date would corrupt both with no visible signal, so this is a
        // hard failure, not a fallback to today's date.
        if (parsed.Date is null || !DateOnly.TryParseExact(parsed.Date, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            throw new InvoiceExtractionException("The model could not determine a valid invoice date from the document.");
        }

        // EXT- prefix is deliberately distinct from the INV- convention
        // seed/pool data uses, so a synthesized id stays visually
        // recognizable as "not read off the document" wherever it's shown.
        var invoiceId = string.IsNullOrWhiteSpace(parsed.InvoiceNumber)
            ? $"EXT-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}"
            : parsed.InvoiceNumber.Trim();

        var supplier = string.IsNullOrWhiteSpace(parsed.Supplier) ? "Unknown supplier" : parsed.Supplier.Trim();
        var customer = string.IsNullOrWhiteSpace(parsed.Customer) ? "Unknown customer" : parsed.Customer.Trim();

        // LineId is synthesized here, never asked of the model: the model
        // can't reliably reference an invoice id it may itself be
        // returning as null, and nothing downstream needs the model's own
        // notion of a line id, it's used purely as an opaque round-trip key.
        var lineItems = parsed.LineItems
            .Select((li, i) => new InvoiceLineItemRequest($"{invoiceId}-L{i + 1}", li.Description, li.AmountGbp))
            .ToList();

        return new InvoiceClassificationRequest(invoiceId, supplier, customer, date, lineItems);
    }
}
