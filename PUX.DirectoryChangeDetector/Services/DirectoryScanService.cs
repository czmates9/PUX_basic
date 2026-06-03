using PUX.DirectoryChangeDetector.Models;

namespace PUX.DirectoryChangeDetector.Services;

public sealed class DirectoryScanService : IDirectoryScanService
{
    private const int MaxFilesPerDirectoryAccordingToAssignment = 100;

    private readonly IFileHashService _fileHashService;
    private readonly ISnapshotStorage _snapshotStorage;

    public DirectoryScanService(
        IFileHashService fileHashService,
        ISnapshotStorage snapshotStorage)
    {
        _fileHashService = fileHashService;
        _snapshotStorage = snapshotStorage;
    }

    public async Task<ScanResult> ScanAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Cesta k adresáři nesmí být prázdná.");

        var normalizedDirectoryPath = Path.GetFullPath(directoryPath);

        if (!Directory.Exists(normalizedDirectoryPath))
            throw new DirectoryNotFoundException($"Adresář '{normalizedDirectoryPath}' neexistuje.");

        ValidateDirectoryFileCounts(normalizedDirectoryPath);

        var previousSnapshot = await _snapshotStorage.LoadAsync(normalizedDirectoryPath, cancellationToken);
        var currentSnapshot = await CreateCurrentSnapshotAsync(normalizedDirectoryPath, cancellationToken);

        ScanResult result;

        if (previousSnapshot is null)
        {
            result = new ScanResult
            {
                IsFirstScan = true,
                DirectoryPath = normalizedDirectoryPath,
                InitialFileCount = currentSnapshot.Files.Count,
                InitialDirectoryCount = currentSnapshot.Directories.Count
            };
        }
        else
        {
            result = CompareSnapshots(previousSnapshot, currentSnapshot);
            result.DirectoryPath = normalizedDirectoryPath;
        }

        currentSnapshot.CreatedAt = previousSnapshot?.CreatedAt ?? DateTimeOffset.UtcNow;
        currentSnapshot.LastScannedAt = DateTimeOffset.UtcNow;

        await _snapshotStorage.SaveAsync(currentSnapshot, cancellationToken);

        return result;
    }

    private async Task<DirectorySnapshot> CreateCurrentSnapshotAsync(
        string directoryPath,
        CancellationToken cancellationToken)
    {
        var snapshot = new DirectorySnapshot
        {
            DirectoryPath = directoryPath
        };

        snapshot.Directories = Directory
            .EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories)
            .Select(path => NormalizeRelativePath(Path.GetRelativePath(directoryPath, path)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var files = Directory
            .EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var filePath in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileInfo = new FileInfo(filePath);
            var relativePath = NormalizeRelativePath(Path.GetRelativePath(directoryPath, filePath));
            var hash = await _fileHashService.ComputeSha256Async(filePath, cancellationToken);

            snapshot.Files.Add(new FileSnapshot
            {
                RelativePath = relativePath,
                Hash = hash,
                SizeInBytes = fileInfo.Length,
                Version = 1
            });
        }

        return snapshot;
    }

    private static ScanResult CompareSnapshots(
        DirectorySnapshot previousSnapshot,
        DirectorySnapshot currentSnapshot)
    {
        var result = new ScanResult();

        var previousFilesByPath = previousSnapshot.Files.ToDictionary(
            x => x.RelativePath,
            StringComparer.OrdinalIgnoreCase);

        var currentFilesByPath = currentSnapshot.Files.ToDictionary(
            x => x.RelativePath,
            StringComparer.OrdinalIgnoreCase);

        foreach (var currentFile in currentSnapshot.Files)
        {
            if (!previousFilesByPath.TryGetValue(currentFile.RelativePath, out var previousFile))
            {
                currentFile.Version = 1;
                result.NewFiles.Add(currentFile);
                continue;
            }

            if (!string.Equals(currentFile.Hash, previousFile.Hash, StringComparison.OrdinalIgnoreCase))
            {
                currentFile.Version = previousFile.Version + 1;
                result.ChangedFiles.Add(currentFile);
            }
            else
            {
                currentFile.Version = previousFile.Version;
            }
        }

        foreach (var previousFile in previousSnapshot.Files)
        {
            if (!currentFilesByPath.ContainsKey(previousFile.RelativePath))
            {
                result.DeletedFiles.Add(previousFile.RelativePath);
            }
        }

        var previousDirectories = previousSnapshot.Directories.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var currentDirectories = currentSnapshot.Directories.ToHashSet(StringComparer.OrdinalIgnoreCase);

        result.DeletedDirectories = previousDirectories
            .Where(directory => !currentDirectories.Contains(directory))
            .OrderBy(directory => directory)
            .ToList();

        result.NewFiles = result.NewFiles.OrderBy(x => x.RelativePath).ToList();
        result.ChangedFiles = result.ChangedFiles.OrderBy(x => x.RelativePath).ToList();
        result.DeletedFiles = result.DeletedFiles.OrderBy(x => x).ToList();

        return result;
    }

    private static string NormalizeRelativePath(string path)
    {
        return path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
    }

    private static void ValidateDirectoryFileCounts(string rootDirectoryPath)
    {
        foreach (var directoryPath in Directory.EnumerateDirectories(rootDirectoryPath, "*", SearchOption.AllDirectories).Prepend(rootDirectoryPath))
        {
            var fileCount = Directory.EnumerateFiles(directoryPath, "*", SearchOption.TopDirectoryOnly).Count();

            if (fileCount > MaxFilesPerDirectoryAccordingToAssignment)
            {
                throw new InvalidOperationException(
                    $"Adresář '{directoryPath}' obsahuje {fileCount} souborů. Zadání předpokládá nejvýše {MaxFilesPerDirectoryAccordingToAssignment} souborů v každém adresáři.");
            }
        }
    }
}
