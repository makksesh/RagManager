namespace Core.Documents.Interfaces;

public interface IRagSearchService
{
    /// <summary>
    /// Находит релевантные чанки в Qdrant и генерирует ответ через Ollama.
    /// </summary>
    Task<string> AskAsync(
        string question,
        string collectionName,
        int topK = 5,
        CancellationToken ct = default);

    IAsyncEnumerable<string> AskStreamingAsync(
        string question,
        string collectionName,
        int topK = 15,
        CancellationToken ct = default);
}