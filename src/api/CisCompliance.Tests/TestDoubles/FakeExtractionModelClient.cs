using CisCompliance.Core.Abstractions;

namespace CisCompliance.Tests.TestDoubles;

// Mirrors FakeClassificationModelClient: returns a fixed raw JSON string
// regardless of input, standing in for a real OpenAI round trip.
internal sealed class FakeExtractionModelClient : IExtractionModelClient
{
    private readonly string _rawResponse;

    public FakeExtractionModelClient(string rawResponse)
    {
        _rawResponse = rawResponse;
    }

    public Task<string> GetExtractionCompletionAsync(
        string systemPrompt, string userInstruction, byte[] fileBytes, string mediaType, string fileName,
        string jsonSchema, string schemaName, CancellationToken cancellationToken = default) =>
        Task.FromResult(_rawResponse);
}
