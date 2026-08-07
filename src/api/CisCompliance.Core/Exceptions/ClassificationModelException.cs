namespace CisCompliance.Core.Exceptions;

// The one exception type the Api layer needs to know about. Infrastructure
// wraps OpenAI SDK failures in this, and ClassificationService wraps
// malformed or incomplete model output in this too, so a controller can
// catch a single well known type without depending on the OpenAI SDK.
public sealed class ClassificationModelException : Exception
{
    public ClassificationModelException(string message) : base(message)
    {
    }

    public ClassificationModelException(string message, Exception inner) : base(message, inner)
    {
    }
}
