using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Interfaces;

/// <summary>
/// Abstraction for discovering and extracting metadata from files within a target directory tree.
/// Demonstrates Interface Segregation Principle (ISP) and testability through abstraction.
/// </summary>
public interface IFileScanner
{
    /// <summary>
    /// Synchronously scans a directory recursively, extracting file metadata while handling I/O exceptions safely.
    /// </summary>
    /// <param name="folderPath">Root folder path to scan.</param>
    /// <param name="cancellationToken">Cancellation token for aborting long scans.</param>
    /// <param name="progress">Optional progress reporter for discovered file counts.</param>
    /// <returns>Read-only list of discovered content files.</returns>
    IReadOnlyList<ContentFile> Scan(
        string folderPath,
        CancellationToken cancellationToken = default,
        IProgress<int>? progress = null);

    /// <summary>
    /// Asynchronously scans a directory offloading work from the UI thread.
    /// </summary>
    Task<IReadOnlyList<ContentFile>> ScanAsync(
        string folderPath,
        CancellationToken cancellationToken = default,
        IProgress<int>? progress = null);
}

