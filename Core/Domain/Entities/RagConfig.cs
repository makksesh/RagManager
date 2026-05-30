namespace Core.Domain.Entities;

public record RagConfig(
    string InferenceModelName,
    string EmbeddingModelName
);