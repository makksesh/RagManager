namespace Core.Ollama.Domain;

public record PullModelProgress(
    string Status,
    string? Digest,
    long? Total,
    long? Completed
);