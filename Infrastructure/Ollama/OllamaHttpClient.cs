using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Core.Ollama.Domain;
using Core.Ollama.Interfaces;
using Infrastructure.Ollama.Dto;
using Infrastructure.Ollama.Mappers;

namespace Infrastructure.Ollama;

public class OllamaHttpClient : IOllamaClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);
        
    public OllamaHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async IAsyncEnumerable<PullModelProgress> PullStreamAsync(
        string model,
        bool insecure = false,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var requestBody = new
        {
            model,
            insecure,
            stream = true
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/pull")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };
        
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        response.EnsureSuccessStatusCode();
        
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var streamReader = new StreamReader(stream);

        while (!streamReader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await streamReader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            PullStatusEventDto? dto = null;
            try
            {
                dto = JsonSerializer.Deserialize<PullStatusEventDto>(line, JsonOptions);
            }
            catch
            {
                
            }
            
            if (dto is null)
                continue;

            yield return new PullModelProgress(
                Status: dto.Status,
                Digest: dto.Digest,
                Total: dto.Total,
                Completed: dto.Completed
            );
        }
    }

    public async Task DeleteModelAsync(string model, CancellationToken ct = default)
    {
        var requestBody = new {model};

        using var request = new HttpRequestMessage(HttpMethod.Delete, "api/delete")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };
        
        using var response = await _httpClient.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"Model '{model}' not found.");
        }
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<OllamaModel>> GetTagsAsync(CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/tags");
        using var response = await _httpClient.SendAsync(request, ct);
        
        response.EnsureSuccessStatusCode();
        
        await using var stream = await response.Content.ReadAsStreamAsync(ct);

        var dto = await JsonSerializer.DeserializeAsync<TagsResponseDto>(
            stream,
            JsonOptions,
            ct);

        if (dto?.Models is null || dto.Models.Count == 0)
            return Array.Empty<OllamaModel>();

        var result = dto.Models
            .Select(m => m.ToDomain())
            .ToList();
        
        return result;
    }
    
    public async Task<IReadOnlyList<float[]>> EmbedAsync(
        string model,
        IReadOnlyList<string> inputs,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Модель недоступна.", nameof(model));
        if (inputs is null || inputs.Count == 0)
            throw new ArgumentException("Строка для эмбеддинга пуста.", nameof(inputs));

        var requestBody = new
        {
            model,
            input = inputs
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/embed")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);

        var dto = await JsonSerializer.DeserializeAsync<OllamaEmbedResponseDto>(
            stream,
            JsonOptions,
            ct);

        if (dto?.Embeddings is null || dto.Embeddings.Count == 0)
            return Array.Empty<float[]>();
        
        var result = new List<float[]>(dto.Embeddings.Count);

        foreach (var vector in dto.Embeddings)
        {
            if (vector is null || vector.Count == 0)
                continue;

            result.Add(vector.ToArray());
        }

        return result;
    }
}