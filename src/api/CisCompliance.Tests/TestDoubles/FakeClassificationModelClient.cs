using CisCompliance.Core.Abstractions;

namespace CisCompliance.Tests.TestDoubles;

// Returns a fixed raw JSON string regardless of what prompt/schema it was
// asked to send, standing in for a real OpenAI round trip so
// ClassificationService can be tested without a network call or API key.
internal sealed class FakeClassificationModelClient : IClassificationModelClient
{
    private readonly string _rawResponse;

    public FakeClassificationModelClient(string rawResponse)
    {
        _rawResponse = rawResponse;
    }

    public Task<string> GetClassificationCompletionAsync(
        string systemPrompt, string userMessage, string jsonSchema, string schemaName,
        CancellationToken cancellationToken = default) => Task.FromResult(_rawResponse);
}
