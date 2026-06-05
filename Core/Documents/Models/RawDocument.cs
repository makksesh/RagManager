namespace Core.Documents.Models;

public record RawDocument(
    string Id,
    string SourcePath,
    string Content
);