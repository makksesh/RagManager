namespace Infrastructure.Ollama.Dto;

public sealed record OllamaEmbedResponseDto(
    string Model,
    List<List<float>> Embeddings
);