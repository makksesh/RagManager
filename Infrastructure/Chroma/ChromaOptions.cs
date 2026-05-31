namespace Infrastructure.Chroma;

public sealed class ChromaOptions
{
    public string Tenant { get; init; } = "default_tenant";
    public string Database { get; init; } = "default_database";
}