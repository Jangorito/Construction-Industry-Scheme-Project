namespace CisCompliance.Core.Abstractions;

// jsonSchema is a plain string, not an OpenAI SDK type. Core stays free of
// any dependency on the OpenAI package or its BinaryData/ClientModel types;
// only the Infrastructure implementation knows those exist.
public interface IClassificationModelClient
{
    Task<string> GetClassificationCompletionAsync(
        string systemPrompt,
        string userMessage,
        string jsonSchema,
        string schemaName,
        CancellationToken cancellationToken = default);
}
