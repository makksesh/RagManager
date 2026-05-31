namespace Infrastructure.Chroma.Dto;

public record ChromaAddRecordsRequest(
    IReadOnlyList<float[]> Embeddings,
    IReadOnlyList<string> Ids,
    IReadOnlyList<string> Documents,
    IReadOnlyList<Dictionary<string, object>>? Metadatas
);