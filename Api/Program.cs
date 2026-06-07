using Infrastructure;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Подключаем все сервисы из Infrastructure (Qdrant, Ollama, etc.)
var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.AddInfrastructure(); // твой существующий метод из DI.cs

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();