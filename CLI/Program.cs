using CLI.Commands;
using Core.Documents.Interfaces;
using Core.Ollama.Interfaces;
using Infrastructure;
using Infrastructure.Documents;
using Infrastructure.Ollama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;


namespace CLI;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        var ct = cts.Token;

        var kernelBuilder = Kernel.CreateBuilder();
        
        kernelBuilder.AddInfrastructure();

        var kernel = kernelBuilder.Build();
        
        var ollamaClient = kernel.Services.GetRequiredService<IOllamaClient>();
        var ingestionService = kernel.Services.GetRequiredService<IDocumentIngestionService>();
        

        while (!ct.IsCancellationRequested)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line is null)
                break;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            var commandName = parts[0].ToLowerInvariant();
            
            if (commandName is "help")
            {
                PrintHelp();
                continue;
            }
            
            ICommand? command = commandName switch
            {
                "pull" when parts.Length >= 2
                    => new PullModelCommand(ollamaClient, parts[1]),
                "delete" when parts.Length >= 2
                    => new DeleteModelCommand(ollamaClient, parts[1]),
                // "tags" => new ListModelsCommand(client),
                "ls" when parts.Length >= 1
                    => new ListModelsCommand(ollamaClient),
                // "set-inference" => new SetInferenceModelCommand(...),

                _ => null
            };

            if (command is null)
            {
                PrintHelp();
                continue;
            }

            var result =  await command.ExecuteAsync(cts.Token);
            
            if (result != 0)
            {
                Console.WriteLine($"Команды выполнилась с кодом {result}");
            }
        }
        
        return 0;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Используйте:");
        Console.WriteLine("> pull <modelName> - загрузка модели по названию");
        Console.WriteLine("> delete <modelName> - удаление модели по названию");
    }
}