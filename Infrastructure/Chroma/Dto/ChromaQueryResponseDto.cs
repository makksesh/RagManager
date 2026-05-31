namespace Infrastructure.Chroma.Dto;

public record ChromaQueryResponseDto(
    List<List<string>> Ids,
    // на будущее List<List<List<float>>> Embeddings,
    List<List<string>> Documents,
    List<List<Dictionary<string, object>>> Metadatas,
    List<List<double>> Distances
);