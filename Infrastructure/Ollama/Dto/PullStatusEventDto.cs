namespace Infrastructure.Ollama.Dto;

public record PullStatusEventDto(
    string Status,
    string? Digest,
    long? Total,
    long? Completed
);