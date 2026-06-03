using PUX.DirectoryChangeDetector.Models;

namespace PUX.DirectoryChangeDetector.Services;

public interface IDirectoryScanService
{
    Task<ScanResult> ScanAsync(string directoryPath, CancellationToken cancellationToken = default);
}
