using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Core.Documents.Interfaces;
using Core.Documents.Models;

namespace Infrastructure.Documents;

public class FileSystemDocumentSource : IDocumentSource
{
    private static readonly string[] SupportedExtensions = new[]
    {
        ".txt",
        ".md"
        // позже ".pdf"
    };

    public async IAsyncEnumerable<RawDocument> ReadAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Path '{path}' does not exist.");
        }
        
        if (File.Exists(path))
        {
            if (IsSupported(path))
                yield return await ReadFileAsync(path, ct);
            yield break;
        }
        
        if (Directory.Exists(path))
        {
            foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                ct.ThrowIfCancellationRequested();
                
                if (!IsSupported(file))
                    continue;

                yield return await ReadFileAsync(file, ct);
            }
        }
    }
    
    private static bool IsSupported(string filePath)
        => SupportedExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase);

    private static async Task<RawDocument> ReadFileAsync(string filePath, CancellationToken ct)
    {
        var content = await File.ReadAllTextAsync(filePath, ct);
        var id = Path.GetFileNameWithoutExtension(filePath);

        return new RawDocument(
            id,
            filePath,
            content
        );
    }

}