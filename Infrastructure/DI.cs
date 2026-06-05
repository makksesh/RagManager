using Infrastructure.Documents;
using Core.Documents.Interfaces;
using Core.Ollama.Interfaces;
using Infrastructure.Ollama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IKernelBuilder AddInfrastructure(
        this IKernelBuilder builder,
        string ollamaEndpoint = "http://localhost:11434",
        string qdrantHost = "localhost")
    {
        // Ollama CLI-клиент (для pull/delete/ls моделей)
        builder.Services.AddHttpClient<IOllamaClient, OllamaHttpClient>(client =>
        {
            client.BaseAddress = new Uri(ollamaEndpoint);
        });

        // Эмбеддинги через Ollama
        builder.AddOllamaEmbeddingGenerator(
            modelId: "nomic-embed-text",
            endpoint: new Uri(ollamaEndpoint));
        
        builder.AddOllamaChatCompletion(
            modelId: "llama3.2",       // или любая другая модель
            endpoint: new Uri(ollamaEndpoint));

        // Qdrant VectorStore
        builder.Services.AddQdrantVectorStore(qdrantHost);

        // Свои сервисы
        builder.Services.AddSingleton<IDocumentSource, FileSystemDocumentSource>();
        builder.Services.AddScoped<IDocumentIngestionService, SemanticKernelIngestionService>();
        builder.Services.AddScoped<IRagSearchService, RagSearchService>();

        return builder;
    }
}