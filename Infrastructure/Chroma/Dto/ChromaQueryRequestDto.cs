namespace Infrastructure.Chroma.Dto;

public record ChromaQueryRequestDto(
    IReadOnlyList<IReadOnlyList<float>> QueryEmbeddings,
    int NResults,
    IReadOnlyDictionary<string, object>? Where = null,
    IReadOnlyList<string>? Include = null
);