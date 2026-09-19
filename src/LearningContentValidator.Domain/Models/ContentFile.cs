namespace LearningContentValidator.Domain.Models;

/// <summary>
/// Domain model encapsulating metadata for a content file discovered during package scanning.
/// Demonstrates C# properties, encapsulation, static members, and extension-to-type dictionary lookup.
/// </summary>
public sealed class ContentFile
{
    private static readonly Dictionary<string, ContentFileType> ExtensionMapping =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = ContentFileType.Pdf,
            [".html"] = ContentFileType.Html,
            [".htm"] = ContentFileType.Html,
            [".json"] = ContentFileType.Json,
            [".png"] = ContentFileType.Image,
            [".jpg"] = ContentFileType.Image,
            [".jpeg"] = ContentFileType.Image,
            [".gif"] = ContentFileType.Image,
            [".svg"] = ContentFileType.Image,
            [".mp4"] = ContentFileType.Video,
            [".webm"] = ContentFileType.Video,
            [".mov"] = ContentFileType.Video,
            [".zip"] = ContentFileType.Zip
        };

    public string FileName { get; }
    public string FullPath { get; }
    public string Extension { get; }
    public long FileSize { get; }
    public DateTimeOffset LastModified { get; }
    public ContentFileType FileType { get; }

    public ContentFile(
        string fileName,
        string fullPath,
        string extension,
        long fileSize,
        DateTimeOffset lastModified,
        ContentFileType? fileType = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentException("Full path cannot be null or empty.", nameof(fullPath));

        FileName = fileName;
        FullPath = fullPath;
        Extension = extension ?? string.Empty;
        FileSize = Math.Max(0, fileSize);
        LastModified = lastModified;
        FileType = fileType ?? ResolveFileType(Extension);
    }

    /// <summary>
    /// Resolves file type from extension using O(1) case-insensitive dictionary lookup.
    /// Demonstrates C# Dictionary (analogous to std::unordered_map in C++).
    /// </summary>
    public static ContentFileType ResolveFileType(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return ContentFileType.Unknown;

        string normalized = extension.StartsWith('.') ? extension : "." + extension;
        return ExtensionMapping.TryGetValue(normalized, out var type) ? type : ContentFileType.Unknown;
    }

    /// <summary>
    /// Returns human-readable representation of file size (B, KB, MB, GB).
    /// </summary>
    public string FormattedSize => FormatByteSize(FileSize);

    public static string FormatByteSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }

    public override string ToString() => $"{FileName} ({FileType}, {FormattedSize})";
}

