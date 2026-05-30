using Core.Ollama.Interfaces;

namespace CLI.Commands;

public class PullModelCommand : ICommand
{
    private readonly IOllamaClient _ollamaClient;
    private readonly string _modelName;
    
    public PullModelCommand(IOllamaClient ollamaClient, string modelName)
    {
        _ollamaClient = ollamaClient;
        _modelName = modelName;
    }
    
    public async Task<int> ExecuteAsync(CancellationToken ct = default)
    {
        Console.WriteLine($"Загружаемая модель: {_modelName}");

        await foreach (var p in _ollamaClient.PullStreamAsync(_modelName, ct: ct))
        {
            var total = p.Total ?? 0;
            var completed = p.Completed ?? 0;
            double? percent = total > 0 ? (double)completed / total : null;

            Console.WriteLine(
                percent is null
                    ? $"\r{p.Status, -40}"
                    : $"\r{p.Status, -20} {percent:0.0}%");
        }

        Console.WriteLine();
        Console.WriteLine("Модель загружена ✅");
        
        return 0;
    }
}