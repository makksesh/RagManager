using Core.Documents.Interfaces;
using Core.Documents.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Text;

namespace Infrastructure.Documents;

public class SemanticKernelIngestionService : IDocumentIngestionService
{
    private readonly IDocumentSource _source;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embedder;
    private readonly VectorStore _vectorStore;

    public SemanticKernelIngestionService(
        IDocumentSource source,
        IEmbeddingGenerator<string, Embedding<float>> embedder,
        VectorStore vectorStore)
    {
        _source = source;
        _embedder = embedder;
        _vectorStore = vectorStore;
    }

    public async Task IngestAsync(string path, string collectionName, CancellationToken ct = default)
    {
        var collection = _vectorStore.GetCollection<ulong, DocumentChunk>(collectionName);
        await collection.EnsureCollectionExistsAsync(ct);
        
        await foreach (var doc in _source.ReadAsync(path, ct))
        {
            var lines = TextChunker.SplitPlainTextLines(doc.Content, maxTokensPerLine: 100);
            var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerParagraph: 500);
            
            GeneratedEmbeddings<Embedding<float>> results = await _embedder.GenerateAsync(chunks, cancellationToken: ct);

            foreach (var (embedding, i) in results.Select((e, i) => (e, i)))
            {
                await collection.UpsertAsync(new DocumentChunk
                {
                    Id = (ulong)((long)HashCode.Combine(doc.SourcePath, i) & long.MaxValue),
                    Text = chunks[i],
                    Source = doc.SourcePath,
                    Embedding = embedding.Vector
                }, cancellationToken: ct);
            }
        }
    }
}