namespace Core.Chroma;

public interface IVectorStore
{
    Task AddAsync(
        string collectionName,
        IReadOnlyList<string> ids,
        IReadOnlyList<float[]> embeddings,
        IReadOnlyList<string> documents,
        IReadOnlyList<Dictionary<string, object>>? metadatas = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<(string Id, string Document, Dictionary<string, object>? Metadata)>> QueryAsync(
        string collectionName,
        float[] queryEmbedding,
        int topK,
        CancellationToken ct = default);
}