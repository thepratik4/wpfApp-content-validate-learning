namespace LearningContentValidator.Domain.Models;

/// <summary>
/// Summary statistics compiled at the end of a package validation run.
/// Demonstrates LINQ aggregation (Count, Sum) on in-memory collections (LINQ to Objects).
/// </summary>
public sealed class ValidationSummary
{
    public int TotalFiles { get; }
    public int ValidFiles { get; }
    public int InvalidFiles { get; }
    public int WarningCount { get; }
    public int ErrorCount { get; }
    public TimeSpan Duration { get; }

    public ValidationSummary(
        int totalFiles,
        int validFiles,
        int invalidFiles,
        int warningCount,
        int errorCount,
        TimeSpan duration)
    {
        TotalFiles = totalFiles;
        ValidFiles = validFiles;
        InvalidFiles = invalidFiles;
        WarningCount = warningCount;
        ErrorCount = errorCount;
        Duration = duration;
    }

    /// <summary>
    /// Factory method computing aggregate statistics from validation results using LINQ to Objects.
    /// Notice how LINQ simplifies complex iterations into declarative, readable expressions.
    /// </summary>
    public static ValidationSummary Create(IReadOnlyList<ValidationResult> results, TimeSpan duration)
    {
        if (results == null) throw new ArgumentNullException(nameof(results));

        int total = results.Count;
        int valid = results.Count(r => r.IsValid);
        int invalid = total - valid;
        int warnings = results.Sum(r => r.WarningCount);
        int errors = results.Sum(r => r.ErrorCount);

        return new ValidationSummary(total, valid, invalid, warnings, errors, duration);
    }

    public string FormattedDuration =>
        Duration.TotalSeconds < 1.0
            ? $"{Duration.TotalMilliseconds:F0} ms"
            : $"{Duration.TotalSeconds:F2} sec";

    public static ValidationSummary Empty => new(0, 0, 0, 0, 0, TimeSpan.Zero);
}

