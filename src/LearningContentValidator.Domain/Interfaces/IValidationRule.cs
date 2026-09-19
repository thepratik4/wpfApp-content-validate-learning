using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation;

namespace LearningContentValidator.Domain.Interfaces;

/// <summary>
/// Extensible validation rule contract adhering to the Open/Closed Principle (OCP).
/// New rules can be added to the engine simply by implementing this interface and registering
/// them in the Dependency Injection container, without modifying existing validation engine code.
/// Analogous to a C++ pure virtual interface class.
/// </summary>
public interface IValidationRule
{
    /// <summary>
    /// Unique identifier / display name for the rule.
    /// </summary>
    string RuleName { get; }

    /// <summary>
    /// Brief description of the criteria this rule enforces.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Validates a single content file within the current package validation context.
    /// </summary>
    /// <param name="file">File to validate.</param>
    /// <param name="context">Shared context holding package options and file indices.</param>
    /// <param name="cancellationToken">Cancellation token to abort validation.</param>
    /// <returns>Collection of zero or more issues identified by this rule.</returns>
    Task<IReadOnlyList<ValidationIssue>> ValidateAsync(
        ContentFile file,
        ValidationContext context,
        CancellationToken cancellationToken = default);
}
