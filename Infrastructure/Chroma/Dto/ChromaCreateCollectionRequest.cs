namespace Infrastructure.Chroma.Dto;

public record ChromaCreateCollectionRequest(
    string CollectionName,
    Dictionary<string, object>? Metadata
);