namespace CisCompliance.Core.Exceptions;

// Mirrors ClassificationModelException. The one exception type the Api
// layer needs to know about for extraction failures, whether the failure
// is an OpenAI call error or the model's output not making sense (a bad
// date, no line items).
public sealed class InvoiceExtractionException : Exception
{
    public InvoiceExtractionException(string message) : base(message)
    {
    }

    public InvoiceExtractionException(string message, Exception inner) : base(message, inner)
    {
    }
}
