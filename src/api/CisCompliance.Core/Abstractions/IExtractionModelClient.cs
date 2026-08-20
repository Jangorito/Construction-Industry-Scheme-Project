namespace CisCompliance.Core.Abstractions;

// Plain bytes/strings in, string JSON out, same discipline as
// IClassificationModelClient: no OpenAI SDK types (BinaryData,
// ChatMessageContentPart, etc) leak into Core, only the Infrastructure
// implementation knows those exist.
public interface IExtractionModelClient
{
    Task<string> GetExtractionCompletionAsync(
        string systemPrompt,
        string userInstruction,
        byte[] fileBytes,
        string mediaType,
        string fileName,
        string jsonSchema,
        string schemaName,
        CancellationToken cancellationToken = default);
}
