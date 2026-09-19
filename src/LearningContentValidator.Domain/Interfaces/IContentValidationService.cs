using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Interfaces;

/// <summary>
/// Service contract for orchestrating rule execution over a set of scanned content files.
/// Injected into ViewModels or orchestration services (Dependency Inversion Principle).
/// </summary>
public interface IContentValidationService
{
    /// <summary>
    /// Validates a collection of content files against all registered validation rules.
    /// Supports asynchronous execution, cooperative cancellation, and progress reporting.
    /// </summary>
    Task<ValidationSessionResult> ValidateFilesAsync(
        IReadOnlyList<ContentFile> files,
        ValidationOptions? overrideOptions = null,
        IProgress<ValidationProgressReport>? progress = null,
        CancellationToken cancellationToken = default);
}

