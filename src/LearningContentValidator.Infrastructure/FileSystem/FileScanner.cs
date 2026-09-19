using Microsoft.Extensions.Logging;
using LearningContentValidator.Domain.Interfaces;
using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Infrastructure.FileSystem;

/// <summary>
/// Robust, resilient recursive directory scanner that extracts metadata for content files.
/// Implements enterprise fault-tolerance: inaccessible or locked files/folders are safely logged
/// and skipped without halting or aborting the overall scanning process.
/// </summary>
public sealed class FileScanner : IFileScanner
{
    private readonly ILogger<FileScanner> _logger;

    /// <summary>
    /// Set of directory names and file patterns to ignore (system metadata, version control, temp files).
    /// Using HashSet with OrdinalIgnoreCase guarantees O(1) matching.
    /// </summary>
    private static readonly HashSet<string> IgnoredDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".vs",
        ".idea",
        ".svn",
        "bin",
        "obj",
        "node_modules",
        "$RECYCLE.BIN",
        "System Volume Information"
    };

    private static readonly HashSet<string> IgnoredFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "desktop.ini",
        "thumbs.db",
        ".ds_store"
    };

    public FileScanner(ILogger<FileScanner> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Synchronously scans the folder recursively.
    /// </summary>
    public IReadOnlyList<ContentFile> Scan(
        string folderPath,
        CancellationToken cancellationToken = default,
        IProgress<int>? progress = null)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Folder path cannot be null or whitespace.", nameof(folderPath));

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"The target directory does not exist: '{folderPath}'.");

        _logger.LogInformation("Beginning recursive scan on root path: {FolderPath}", folderPath);

        var results = new List<ContentFile>();
        var rootDir = new DirectoryInfo(folderPath);

        ScanDirectoryRecursively(rootDir, results, cancellationToken, progress);

        _logger.LogInformation("Scan completed successfully. Discovered {Count} content files in {FolderPath}",
            results.Count, folderPath);

        return results.AsReadOnly();
    }

    /// <summary>
    /// Asynchronously offloads scanning to a background task so UI remains responsive.
    /// </summary>
    public Task<IReadOnlyList<ContentFile>> ScanAsync(
        string folderPath,
        CancellationToken cancellationToken = default,
        IProgress<int>? progress = null)
    {
        return Task.Run(() => Scan(folderPath, cancellationToken, progress), cancellationToken);
    }

    /// <summary>
    /// Resilient recursive crawler. Catches exceptions at directory and file boundaries.
    /// </summary>
    private void ScanDirectoryRecursively(
        DirectoryInfo currentDir,
        List<ContentFile> results,
        CancellationToken cancellationToken,
        IProgress<int>? progress)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 1. Scan files within current directory
        FileInfo[] files;
        try
        {
            files = currentDir.GetFiles();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Skipping unauthorized directory '{Path}': {Message}", currentDir.FullName, ex.Message);
            return;
        }
        catch (DirectoryNotFoundException ex)
        {
            _logger.LogWarning("Directory no longer exists '{Path}': {Message}", currentDir.FullName, ex.Message);
            return;
        }
        catch (PathTooLongException ex)
        {
            _logger.LogWarning("Path exceeds system limits '{Path}': {Message}", currentDir.FullName, ex.Message);
            return;
        }
        catch (IOException ex)
        {
            _logger.LogWarning("I/O error accessing directory '{Path}': {Message}", currentDir.FullName, ex.Message);
            return;
        }

        foreach (var fileInfo in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldIgnoreFile(fileInfo))
                continue;

            try
            {
                var contentFile = new ContentFile(
                    fileName: fileInfo.Name,
                    fullPath: fileInfo.FullName,
                    extension: fileInfo.Extension,
                    fileSize: fileInfo.Length,
                    lastModified: fileInfo.LastWriteTimeUtc
                );

                results.Add(contentFile);
                progress?.Report(results.Count);
            }
            catch (FileNotFoundException)
            {
                // File was deleted or moved between GetFiles() and reading metadata
                _logger.LogWarning("File was removed during scan: {Path}", fileInfo.FullName);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Access denied reading file attributes '{Path}': {Message}", fileInfo.FullName, ex.Message);
            }
            catch (IOException ex)
            {
                _logger.LogWarning("I/O exception reading file metadata '{Path}': {Message}", fileInfo.FullName, ex.Message);
            }
        }

        // 2. Recurse into child subdirectories
        DirectoryInfo[] subDirectories;
        try
        {
            subDirectories = currentDir.GetDirectories();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Access denied enumerating subdirectories in '{Path}': {Message}", currentDir.FullName, ex.Message);
            return;
        }
        catch (IOException ex)
        {
            _logger.LogWarning("I/O exception enumerating subdirectories in '{Path}': {Message}", currentDir.FullName, ex.Message);
            return;
        }

        foreach (var subDir in subDirectories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldIgnoreDirectory(subDir))
                continue;

            ScanDirectoryRecursively(subDir, results, cancellationToken, progress);
        }
    }

    private static bool ShouldIgnoreDirectory(DirectoryInfo dir)
    {
        if (IgnoredDirectoryNames.Contains(dir.Name))
            return true;

        // Skip hidden or system folders
        if ((dir.Attributes & FileAttributes.Hidden) != 0 || (dir.Attributes & FileAttributes.System) != 0)
            return true;

        return false;
    }

    private static bool ShouldIgnoreFile(FileInfo file)
    {
        if (IgnoredFileNames.Contains(file.Name))
            return true;

        // Skip temporary Office lock files (e.g. ~$document.docx)
        if (file.Name.StartsWith("~$", StringComparison.Ordinal))
            return true;

        return false;
    }
}

