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
}