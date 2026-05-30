using Core.Ollama.Domain;
using Infrastructure.Ollama.Dto;

namespace Infrastructure.Ollama.Mappers;

public class OllamaModelMappers
{
    public static OllamaModel ToDomain(TagModelDto dto)
    {
        var name = dto.Name ?? dto.Model ?? "unknown";

        DateTimeOffset? modifiedAt = null;

        if (!string.IsNullOrWhiteSpace(dto.ModifiedAt) &&
            DateTimeOffset.TryParse(dto.ModifiedAt, out var parsedModifiedAt))
        {
            modifiedAt = parsedModifiedAt;
        }

        return new OllamaModel(
            Name: name,
            RemoteModel: dto.RemoteModel,
            RemoteHost: dto.RemoteHost,
            SizeBytes: dto.Size ?? 0,
            ModifiedAt: modifiedAt,
            Digest: dto.Digest,
            Format: dto.Details?.Format,
            Family: dto.Details?.Family,
            Families: dto.Details?.Families ?? new List<string>(),
            ParameterSize: dto.Details?.ParameterSize,
            QuantizationLevel: dto.Details?.QuantizationLevel
        );
    }
}