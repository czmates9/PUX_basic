namespace PUX.DirectoryChangeDetector.Models;

public sealed class DirectorySnapshot
{
    public required string DirectoryPath { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastScannedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<string> Directories { get; set; } = new();

    public List<FileSnapshot> Files { get; set; } = new();
}
