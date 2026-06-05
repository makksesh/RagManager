namespace Core.Documents.Interfaces;

public interface IDocumentIngestionService
{
    Task IngestAsync(string filePath, string collectionName, CancellationToken ct = default);
}