using Core.Documents.Models;

namespace Core.Documents.Interfaces;

public interface IDocumentSource
{
    IAsyncEnumerable<RawDocument> ReadAsync(
        string path,
        CancellationToken ct = default);
    
    
}