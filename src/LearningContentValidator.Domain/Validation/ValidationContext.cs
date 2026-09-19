using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Validation;

/// <summary>
/// Execution context passed to validation rules during a validation run.
/// Pre-computes package-wide indices (e.g. file name lookups via LINQ ToLookup) so that
/// cross-file rules (like duplicate detection) operate in O(1) time without O(N^2) file rescans.
/// </summary>
public sealed class ValidationContext
{
    public ValidationOptions Options { get; }
    public IReadOnlyList<ContentFile> AllFiles { get; }

    /// <summary>
    /// Fast case-insensitive lookup of all files by name across the entire package.
    /// Demonstrates LINQ ToLookup (analogous to std::unordered_multimap in C++).
    /// </summary>
    public ILookup<string, ContentFile> FilesByName { get; }

    public ValidationContext(ValidationOptions options, IReadOnlyList<ContentFile> allFiles)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        AllFiles = allFiles ?? throw new ArgumentNullException(nameof(allFiles));

        FilesByName = AllFiles.ToLookup(f => f.FileName, StringComparer.OrdinalIgnoreCase);
    }
}

