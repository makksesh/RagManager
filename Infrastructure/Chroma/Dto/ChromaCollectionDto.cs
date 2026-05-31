namespace Infrastructure.Chroma.Dto;

public record ChromaCollectionDto(
    string Id,
    string Name,
    int? Dimension,
    string? Tenant,
    string? Database,
    Dictionary<string, object>? Metadata
);