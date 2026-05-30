namespace Core.Ollama.Domain;

public record OllamaModel(
    string Name,
    string? RemoteModel,
    string? RemoteHost,
    long SizeBytes,
    DateTimeOffset? ModifiedAt,
    string? Digest,
    string? Format,
    string? Family,
    IReadOnlyList<string> Families,
    string? ParameterSize,
    string? QuantizationLevel
);