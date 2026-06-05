using Core.Documents.Interfaces;

namespace CLI.Commands;

public class AskCommand : ICommand
{
    private readonly IRagSearchService _rag;
    private readonly string _question;
    private readonly string _collection;

    public AskCommand(IRagSearchService rag, string collection, string question)
    {
        _rag = rag;
        _collection = collection;
        _question = question;
    }

    public async Task<int> ExecuteAsync(CancellationToken ct = default)
    {
        Console.WriteLine($"Ищу в коллекции '{_collection}'...\n");
        var answer = await _rag.AskAsync(_question, _collection, ct: ct);
        Console.WriteLine($"Ответ:\n{answer}");
        return 0;
    }
}