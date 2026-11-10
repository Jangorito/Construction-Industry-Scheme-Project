namespace CisCompliance.Core.Exceptions;

// Summary:
// Represents an exception thrown when the classification model fails, can be thrown 
//  by the ClassificationService or the OpenAI SDK. If thrown by model it is wrapped
//  in this exception type so API layer doesn't need to know model's exception types.
public sealed class ClassificationModelException : Exception
{
    public ClassificationModelException(string message) : base(message)
    {
    }

    public ClassificationModelException(string message, Exception inner) : base(message, inner)
    {
    }
}
