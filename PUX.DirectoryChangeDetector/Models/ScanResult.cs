namespace PUX.DirectoryChangeDetector.Models;

public sealed class ScanResult
{
    public bool IsFirstScan { get; set; }

    public string? DirectoryPath { get; set; }

    public DateTimeOffset ScannedAt { get; set; } = DateTimeOffset.Now;

    public int InitialFileCount { get; set; }

    public int InitialDirectoryCount { get; set; }

    public List<FileSnapshot> NewFiles { get; set; } = new();

    public List<FileSnapshot> ChangedFiles { get; set; } = new();

    public List<string> DeletedFiles { get; set; } = new();

    public List<string> DeletedDirectories { get; set; } = new();

    public bool HasAnyChange =>
        NewFiles.Count > 0 ||
        ChangedFiles.Count > 0 ||
        DeletedFiles.Count > 0 ||
        DeletedDirectories.Count > 0;
}
