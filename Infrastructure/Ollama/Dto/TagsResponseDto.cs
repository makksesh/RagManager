namespace Infrastructure.Ollama.Dto;

public record TagsResponseDto(
    List<TagModelDto> Models
);

public record TagModelDto (
    string? Name,
    string? Model,
    string? RemoteModel,
    string? RemoteHost,
    string? ModifiedAt,
    long? Size,
    string? Digest,
    TagModelDetailsDto? Details
);

public record TagModelDetailsDto(
    string? Format,
    string? Family,
    List<string>? Families,
    string? ParameterSize,
    string? QuantizationLevel
);