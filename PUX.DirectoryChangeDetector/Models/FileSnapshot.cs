namespace PUX.DirectoryChangeDetector.Models;

public sealed class FileSnapshot
{
    public required string RelativePath { get; set; }

    public required string Hash { get; set; }

    public long SizeInBytes { get; set; }

    public int Version { get; set; } = 1;
}
