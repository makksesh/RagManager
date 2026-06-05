using Core.Documents.Interfaces;

namespace CLI.Commands;

public class IngestCommand : ICommand
{
    private readonly IDocumentIngestionService _ingestion;
    private readonly string _path;
    private readonly string _collection;

    public IngestCommand(IDocumentIngestionService ingestion, string path, string collection)
    {
        _ingestion = ingestion;
        _path = path;
        _collection = collection;
    }

    public async Task<int> ExecuteAsync(CancellationToken ct)
    {
        Console.WriteLine($"Индексирую '{_path}' → коллекция '{_collection}'...");
        await _ingestion.IngestAsync(_path, _collection, ct);
        Console.WriteLine("✓ Готово!");
        return 0;
    }
}