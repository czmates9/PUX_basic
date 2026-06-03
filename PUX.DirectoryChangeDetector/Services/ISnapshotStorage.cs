using PUX.DirectoryChangeDetector.Models;

namespace PUX.DirectoryChangeDetector.Services;

public interface ISnapshotStorage
{
    Task<DirectorySnapshot?> LoadAsync(string directoryPath, CancellationToken cancellationToken = default);

    Task SaveAsync(DirectorySnapshot snapshot, CancellationToken cancellationToken = default);
}
