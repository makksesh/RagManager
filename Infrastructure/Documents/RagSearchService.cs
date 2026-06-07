using System.Runtime.CompilerServices;
using Core.Documents.Interfaces;
using Core.Documents.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace Infrastructure.Documents;

public class RagSearchService : IRagSearchService
{
    private readonly VectorStore _vectorStore;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embedder;
    private readonly IChatClient _chat;

    public RagSearchService(
        VectorStore vectorStore,
        IEmbeddingGenerator<string, Embedding<float>> embedder,
        IChatClient chat)
    {
        _vectorStore = vectorStore;
        _embedder = embedder;
        _chat = chat;
    }

    public async Task<string> AskAsync(
        string question,
        string collectionName,
        int topK = 5,
        CancellationToken ct = default)
    {
        // 1. Эмбеддинг вопроса
        var queryEmbedding = await _embedder.GenerateAsync(question, cancellationToken: ct);

        // 2. Поиск похожих чанков в Qdrant
        var collection = _vectorStore.GetCollection<ulong, DocumentChunk>(collectionName);
        var searchResults = collection.SearchAsync(
            queryEmbedding.Vector,
            top: topK,
            cancellationToken: ct);

        // 3. Собираем контекст
        var contextParts = new List<string>();
        await foreach (var result in searchResults)
        {
            contextParts.Add($"[{result.Record.Source}]\n{result.Record.Text}");
        }

        if (contextParts.Count == 0)
            return "Ничего не найдено. Сначала проиндексируйте документы через \"ingest\"";

        // 4. Prompt для LLM
        var prompt = $"""
                      Контекст из документов:
                      {string.Join("\n\n", contextParts)}

                      Вопрос: {question}

                      Ответь на основе контекста выше.
                      """;

        // 5. Ответ через Ollama
        var response = await _chat.GetResponseAsync(prompt, cancellationToken: ct);
        return response.Text;
    }
    
    // В RagSearchService.cs добавить метод:
    public async IAsyncEnumerable<string> AskStreamingAsync(
        string question,
        string collectionName,
        int topK = 15,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var queryEmbedding = await _embedder.GenerateAsync(question, cancellationToken: ct);

        var collection = _vectorStore.GetCollection<ulong, DocumentChunk>(collectionName);
        var searchResults = collection.SearchAsync(
            queryEmbedding.Vector,
            top: topK,
            cancellationToken: ct);

        var contextParts = new List<string>();
        await foreach (var result in searchResults)
            contextParts.Add($"[{result.Record.Source}]\n{result.Record.Text}");

        var prompt = $"Контекст:\n{string.Join("\n\n", contextParts)}\n\nВопрос: {question}\n\nОтвет:";

        await foreach (var chunk in _chat.GetStreamingResponseAsync(prompt, cancellationToken: ct))
            yield return chunk.Text ?? "";
    }
}