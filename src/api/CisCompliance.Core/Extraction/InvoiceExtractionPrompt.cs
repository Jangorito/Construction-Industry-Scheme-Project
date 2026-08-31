namespace CisCompliance.Core.Extraction;

// Fully static, unlike ClassificationPrompt: extraction has no rules doc or
// prior invoice data to interpolate per request, so there's no
// BuildSystemPrompt(...) method here, just the constants.
public static class InvoiceExtractionPrompt
{
    public const string SystemPrompt = """
        You are extracting structured data from a UK subcontractor invoice document (a PDF or a photo/scan of one). Read the document carefully and extract exactly the fields in the response schema.

        For invoice_number, customer, and date: if the document does not clearly show a value, return null for that field rather than guessing or inventing one. This matters most for invoice_number, never fabricate a plausible-looking invoice number, a fabricated one that happens to match a real invoice's number would silently overwrite that real record.

        For date specifically, only return a date you can actually read on the document (an invoice date, issue date, or similar). Do not use today's date or any other date as a substitute if none is printed. Always return it as ISO 8601, "YYYY-MM-DD", regardless of how it's formatted on the document (e.g. a UK-style "28/08/2026" must be returned as "2026-08-28").

        For supplier, extract the name of the company issuing the invoice (doing the work). For customer, extract the name of the company or person being billed, if shown.

        For line_items, extract every distinct chargeable item as its own entry, with its description exactly as written and its amount in pounds sterling exactly as printed, no currency conversion and no rounding beyond what's on the document. Do not invent a line item that isn't there, and do not merge separate line items into one.
        """;

    public const string UserInstruction =
        "Extract the invoice data from the attached document, following the rules in your instructions exactly.";
}
