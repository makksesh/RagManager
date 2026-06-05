using Microsoft.Extensions.VectorData;

namespace Core.Documents.Models;


public class DocumentChunk
{
    [VectorStoreKey]
    public ulong Id { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Text { get; set; } = "";

    [VectorStoreData(IsIndexed = true)]
    public string Source { get; set; } = "";

    [VectorStoreVector(Dimensions: 768, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float>? Embedding { get; set; }
}