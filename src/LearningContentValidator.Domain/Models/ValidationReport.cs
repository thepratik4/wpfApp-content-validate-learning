namespace LearningContentValidator.Domain.Models;

/// <summary>
/// Serializable data transfer model representing a complete validation report export.
/// </summary>
public sealed class ValidationReport
{
    public string ScannedFolder { get; }
    public DateTimeOffset GeneratedAt { get; }
    public ValidationSummary Summary { get; }
    public IReadOnlyList<ValidationResult> Results { get; }
    public ValidationOptions OptionsUsed { get; }

    public ValidationReport(
        string scannedFolder,
        ValidationSummary summary,
        IReadOnlyList<ValidationResult> results,
        ValidationOptions optionsUsed,
        DateTimeOffset? generatedAt = null)
    {
        ScannedFolder = scannedFolder ?? string.Empty;
        Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        Results = results ?? throw new ArgumentNullException(nameof(results));
        OptionsUsed = optionsUsed ?? throw new ArgumentNullException(nameof(optionsUsed));
        GeneratedAt = generatedAt ?? DateTimeOffset.UtcNow;
    }
}

