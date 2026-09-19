namespace LearningContentValidator.Domain.Models;

/// <summary>
/// Encapsulates the overall result of a validation session, containing individual file outcomes and aggregate metrics.
/// </summary>
public sealed class ValidationSessionResult
{
    public IReadOnlyList<ValidationResult> Results { get; }
    public ValidationSummary Summary { get; }
    public ValidationOptions OptionsUsed { get; }
    public DateTimeOffset CompletedAt { get; }

    public ValidationSessionResult(
        IReadOnlyList<ValidationResult> results,
        ValidationSummary summary,
        ValidationOptions optionsUsed)
    {
        Results = results ?? throw new ArgumentNullException(nameof(results));
        Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        OptionsUsed = optionsUsed ?? throw new ArgumentNullException(nameof(optionsUsed));
        CompletedAt = DateTimeOffset.UtcNow;
    }
}

