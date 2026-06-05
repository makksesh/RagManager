using Core.Ollama.Domain;

namespace Core.Ollama.Interfaces;

public interface IOllamaClient
{
    IAsyncEnumerable<PullModelProgress> PullStreamAsync(
        string model,
        bool insecure = false,
        CancellationToken ct = default);
    
    Task DeleteModelAsync(string model, CancellationToken ct = default);
    
    Task<IReadOnlyList<OllamaModel>> GetTagsAsync(CancellationToken ct = default);
    
    Task<IReadOnlyList<float[]>> EmbedAsync(
        string model,
        IReadOnlyList<string> inputs,
        CancellationToken ct = default);
}