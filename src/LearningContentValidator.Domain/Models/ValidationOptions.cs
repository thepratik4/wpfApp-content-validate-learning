namespace LearningContentValidator.Domain.Models;

/// <summary>
/// Configurable options governing the validation engine.
/// Demonstrates sensible defaults, HashSet for O(1) lookups, and clean encapsulation.
/// </summary>
public sealed class ValidationOptions
{
    /// <summary>
    /// Default maximum file size limit: 500 MB.
    /// </summary>
    public const long DefaultMaxFileSizeBytes = 500L * 1024 * 1024;

    public long MaxFileSizeInBytes { get; set; } = DefaultMaxFileSizeBytes;

    /// <summary>
    /// Supported/allowed file extensions for digital learning content.
    /// Using HashSet with StringComparer.OrdinalIgnoreCase gives O(1) case-insensitive lookup,
    /// equivalent to std::unordered_set in C++.
    /// </summary>
    public HashSet<string> AllowedExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".html",
        ".htm",
        ".json",
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".svg",
        ".mp4",
        ".webm",
        ".mov",
        ".zip"
    };

    /// <summary>
    /// Mandatory fields that must appear in package metadata JSON files.
    /// </summary>
    public List<string> RequiredMetadataFields { get; set; } = new()
    {
        "id",
        "title",
        "version",
        "author"
    };

    /// <summary>
    /// System and invalid characters rejected in file names.
    /// </summary>
    public char[] DisallowedFilenameCharacters { get; set; } =
        Path.GetInvalidFileNameChars();

    /// <summary>
    /// Check for 0-byte (empty) files.
    /// </summary>
    public bool CheckZeroByteFiles { get; set; } = true;

    /// <summary>
    /// Check for duplicate file names across the course package.
    /// </summary>
    public bool CheckDuplicateFileNames { get; set; } = true;

    /// <summary>
    /// Maximum allowable length for file name to avoid path exhaustion issues.
    /// </summary>
    public int MaxFileNameLength { get; set; } = 255;
}

