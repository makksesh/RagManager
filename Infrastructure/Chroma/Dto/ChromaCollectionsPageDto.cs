namespace Infrastructure.Chroma.Dto;

public record ChromaCollectionsPageDto(
    List<ChromaCollectionDto> Collections,
    int? Limit,
    int? Offset
);