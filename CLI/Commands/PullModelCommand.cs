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

            const int barWidth = 30;

            string bar;
            string percentText;

            if (percent is null)
            {
                bar = new string('.', barWidth);
                percentText = "";
            }
            else
            {
                var pct = percent.Value * 100;
                var filled = (int)(percent.Value * barWidth);
                filled = Math.Clamp(filled, 0, barWidth);

                bar = new string('#', filled) + new string('-', barWidth - filled);
                percentText = $"{pct:0.0}%";
            }

            var status = p.Status ?? "";
            var line = $"{status,-20} [{bar}] {percentText}";
            
            Console.Write("\r" + line.PadRight(Console.WindowWidth - 1));
        }

        Console.WriteLine();
        Console.WriteLine("Pull completed.");

        Console.WriteLine();
        Console.WriteLine("Модель загружена ✅");
        
        return 0;
    }
}