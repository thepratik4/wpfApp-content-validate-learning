namespace LearningContentValidator.Domain.Models;

/// <summary>
/// Aggregated validation outcome for a specific content file.
/// Demonstrates LINQ queries (Where, Select), read-only collections, and computed properties.
/// </summary>
public sealed class ValidationResult
{
    public string FilePath => File.FullPath;
    public string FileName => File.FileName;
    public ContentFile File { get; }
    public IReadOnlyList<ValidationIssue> Issues { get; }
    public DateTimeOffset ValidationTimestamp { get; }

    /// <summary>
    /// A file is considered valid if there are zero validation errors.
    /// Warnings do not cause validation failure.
    /// </summary>
    public bool IsValid => ErrorCount == 0;

    /// <summary>
    /// Status description: "Valid", "Warning", or "Invalid".
    /// </summary>
    public string StatusText => !IsValid ? "Invalid" : (WarningCount > 0 ? "Warning" : "Valid");

    /// <summary>
    /// Demonstrates LINQ Where & Select to filter and project error messages.
    /// </summary>
    public IReadOnlyList<string> Errors => Issues
        .Where(i => i.Severity == ValidationSeverity.Error)
        .Select(i => $"[{i.RuleName}] {i.Message}")
        .ToList();

    /// <summary>
    /// Demonstrates LINQ Where & Select to filter and project warning messages.
    /// </summary>
    public IReadOnlyList<string> Warnings => Issues
        .Where(i => i.Severity == ValidationSeverity.Warning)
        .Select(i => $"[{i.RuleName}] {i.Message}")
        .ToList();

    public int ErrorCount => Issues.Count(i => i.Severity == ValidationSeverity.Error);
    public int WarningCount => Issues.Count(i => i.Severity == ValidationSeverity.Warning);

    public ValidationResult(ContentFile file, IEnumerable<ValidationIssue>? issues = null)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        Issues = issues?.ToList().AsReadOnly() ?? (IReadOnlyList<ValidationIssue>)Array.Empty<ValidationIssue>();
        ValidationTimestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Helper factory to create a clean passing result.
    /// </summary>
    public static ValidationResult Success(ContentFile file) => new(file);

    public override string ToString() => $"{FileName} - {StatusText} (Errors: {ErrorCount}, Warnings: {WarningCount})";
}

