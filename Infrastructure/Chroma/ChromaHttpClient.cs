using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Core.Chroma;
using Infrastructure.Chroma.Dto;

namespace Infrastructure.Chroma;

public class ChromaHttpClient : IVectorStore
{
    private readonly HttpClient _httpClient;
    private readonly ChromaOptions _options;
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public ChromaHttpClient(HttpClient httpClient,  ChromaOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }
    
    private async Task<ChromaCollectionDto?> TryGetCollectionAsync(
        string collectionName,
        CancellationToken ct = default)
    {
        const int pageSize = 100;
        int offset = 0;
        
        int pagesChecked = 0;
        const int maxPages = 1000; 

        while (pagesChecked < maxPages)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{CollectionsBasePath}?limit={pageSize}&offset={offset}");
            
            using var response = await _httpClient.SendAsync(request, ct);
            
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            var page = await JsonSerializer.DeserializeAsync<ChromaCollectionsPageDto>(
                stream,
                JsonOptions,
                ct);
            
            if (page?.Collections is null || page.Collections.Count == 0)
                return null;

            var found = page.Collections.FirstOrDefault(c => c.Name == collectionName);
            if (found is not null)
                return found;

            offset += pageSize;
            pagesChecked++;
        }
        return null;
    }
    
    private async Task<ChromaCollectionDto> CreateCollectionAsync(
        string collectionName,
        Dictionary<string, object>? metadata = null,
        CancellationToken ct = default)
    {
        var requestBody = new ChromaCreateCollectionRequest(
            collectionName,
            metadata
        );

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            CollectionsBasePath)
        {
            Content = JsonContent.Create(requestBody, options: JsonOptions)
        };

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var created = await JsonSerializer.DeserializeAsync<ChromaCollectionDto>(
            stream,
            JsonOptions,
            ct);

        if (created is null)
            throw new InvalidOperationException("Chroma returned empty response when creating collection.");

        return created;
    }

    private async Task<ChromaCollectionDto> GetOrCreateCollectionAsync(
        string collectionName,
        Dictionary<string, object>? metadata = null,
        CancellationToken ct = default)
    {
        var existing = await TryGetCollectionAsync(collectionName, ct);
        if (existing is not null)
            return existing;
        
        return await CreateCollectionAsync(collectionName, metadata, ct);
    }
    
    private string CollectionsBasePath =>
        $"/api/v2/tenants/{_options.Tenant}/databases/{_options.Database}/collections";

    public async Task AddAsync(
        string collectionName,
        IReadOnlyList<string> ids,
        IReadOnlyList<float[]> embeddings,
        IReadOnlyList<string> documents,
        IReadOnlyList<Dictionary<string, object>>? metadatas = null,
        CancellationToken ct = default)
    {
        var collection = await GetOrCreateCollectionAsync(collectionName, null, ct);
        var collectionId = collection.Id;
        
        var requestBody = new ChromaAddRecordsRequest(
            embeddings,
            ids,
            documents,
            metadatas
        );
        
        var path = $"{CollectionsBasePath}/{collectionId}/add";

        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(requestBody, options: JsonOptions)
        };
        
        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        
    }

    public async Task<IReadOnlyList<(string Id, string Document, Dictionary<string, object>? Metadata)>> QueryAsync(
            string collectionName,
            float[] queryEmbedding,
            int topK, CancellationToken ct = default)
    {
        var collection = await TryGetCollectionAsync(collectionName, ct);
        
        if (collection is null)
            throw new InvalidOperationException(
                $"Collection '{collectionName}' does not exist in Chroma.");
        
        var collectionId = collection.Id;

        var requestBody = new ChromaQueryRequestDto(
            QueryEmbeddings: new List<IReadOnlyList<float>> { queryEmbedding },
            NResults: topK,
            Where: null,
            Include: new List<string> { "documents", "metadatas", "distances" }
        );
        
        var path = $"{CollectionsBasePath}/{collectionId}/query";

        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(requestBody, options: JsonOptions)
        };
        
        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        
        await using var stream = await response.Content.ReadAsStreamAsync(ct);

        var dto = await JsonSerializer.DeserializeAsync<ChromaQueryResponseDto>(
            stream,
            JsonOptions,
            ct);
        
        if (dto is null || dto.Documents.Count == 0 || dto.Documents[0].Count == 0)
            return Array.Empty<(string Id, string Document, Dictionary<string, object>? Metadata)>();

        var ids = dto.Ids[0];
        var docs = dto.Documents[0];
        var metas = dto.Metadatas[0];

        var results = new List<(string Id, string Document, Dictionary<string, object>? Metadata)>(docs.Count);

        for (var i = 0; i < docs.Count; i++)
        {
            results.Add((ids[i], docs[i], metas[i]));
        }

        return results;
    }

    #region Extensions
    
    public async Task<int> GetCollectionsCountAsync(
        string tenant,
        string database,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v2/tenants/{tenant}/databases/{database}/collections_count");

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        return int.Parse(json, CultureInfo.InvariantCulture);
    }

    #endregion
}