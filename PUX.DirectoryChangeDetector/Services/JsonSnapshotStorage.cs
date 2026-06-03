using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PUX.DirectoryChangeDetector.Models;

namespace PUX.DirectoryChangeDetector.Services;

public sealed class JsonSnapshotStorage : ISnapshotStorage
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _storageRoot;
    private readonly ILogger<JsonSnapshotStorage> _logger;

    public JsonSnapshotStorage(IWebHostEnvironment environment, ILogger<JsonSnapshotStorage> logger)
    {
        _logger = logger;
        _storageRoot = Path.Combine(environment.ContentRootPath, "App_Data", "Snapshots");
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<DirectorySnapshot?> LoadAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        var filePath = GetSnapshotFilePath(directoryPath);

        if (!File.Exists(filePath))
            return null;

        await using var stream = File.OpenRead(filePath);

        var snapshot = await JsonSerializer.DeserializeAsync<DirectorySnapshot>(
            stream,
            SerializerOptions,
            cancellationToken);

        _logger.LogInformation("Loaded snapshot from {SnapshotFilePath}", filePath);

        return snapshot;
    }

    public async Task SaveAsync(DirectorySnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var filePath = GetSnapshotFilePath(snapshot.DirectoryPath);

        await using var stream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await JsonSerializer.SerializeAsync(
            stream,
            snapshot,
            SerializerOptions,
            cancellationToken);

        _logger.LogInformation("Saved snapshot to {SnapshotFilePath}", filePath);
    }

    private string GetSnapshotFilePath(string directoryPath)
    {
        var normalizedPath = Path.GetFullPath(directoryPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToLowerInvariant();

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedPath));
        var fileName = Convert.ToHexString(hash);

        return Path.Combine(_storageRoot, $"{fileName}.json");
    }
}
