using CLI.Commands;
using Core.Ollama.Interfaces;
using Infrastructure.Ollama;
using Microsoft.Extensions.DependencyInjection;

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

        // DI-контейнер
        var services = new ServiceCollection();

        services.AddHttpClient<IOllamaClient, OllamaHttpClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:11434");
        });

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IOllamaClient>();

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
                    => new PullModelCommand(client, args[1]),
                "delete" when parts.Length >= 2
                    => new DeleteModelCommand(client, parts[1]),
                // "tags" => new ListModelsCommand(client),
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
        Console.WriteLine("  app pull <modelName>");
    }
}