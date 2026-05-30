using Core.Ollama.Interfaces;

namespace CLI.Commands;

public class ListModelsCommand : ICommand
{
    private readonly IOllamaClient _ollamaClient;

    public ListModelsCommand(IOllamaClient ollamaClient)
    {
        _ollamaClient = ollamaClient;
    }


    public async Task<int> ExecuteAsync(CancellationToken ct = default)
    {
        var models = await _ollamaClient.GetTagsAsync();
        
        if (models.Count == 0)
        {
            Console.WriteLine("Локальных моделей нет.");
            return 0;
        }

        Console.WriteLine($"{"NAME",-20} {"SIZE",-10} {"FAMILY",-10} {"QUANT",-10}");
        foreach (var m in models)
        {
            var sizeMb = m.SizeBytes / (1024.0 * 1024.0);
            Console.WriteLine(
                $"{m.Name,-20} {sizeMb,8:0.0}MB {m.Family,-10} {m.QuantizationLevel,-10}");
        }

        return 0;
    }
}