using Core.Ollama.Interfaces;

namespace CLI.Commands;

public class DeleteModelCommand : ICommand
{
    private readonly IOllamaClient _client;
    private readonly string _modelName;

    public DeleteModelCommand(IOllamaClient client, string modelName)
    {
        _client = client;
        _modelName = modelName;
    }

    public async Task<int> ExecuteAsync(CancellationToken ct = default)
    {
        Console.WriteLine($"Удаление модели: {_modelName}...");

        try
        {
            await _client.DeleteModelAsync(_modelName, ct);
            Console.WriteLine("Deleted.");
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            return 1;
        }
    }
}