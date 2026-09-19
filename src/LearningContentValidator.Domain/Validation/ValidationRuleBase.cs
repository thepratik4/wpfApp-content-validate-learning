using LearningContentValidator.Domain.Interfaces;
using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Validation;

/// <summary>
/// Abstract base class providing common functionality for validation rules.
/// Demonstrates C# inheritance, protected members, and the Template Method pattern.
/// In C++, this corresponds to an abstract base class with pure virtual methods.
/// </summary>
public abstract class ValidationRuleBase : IValidationRule
{
    public abstract string RuleName { get; }
    public abstract string Description { get; }

    /// <summary>
    /// Polymorphic entry point for rule execution.
    /// Derived rules implement the asynchronous validation logic here.
    /// </summary>
    public abstract Task<IReadOnlyList<ValidationIssue>> ValidateAsync(
        ContentFile file,
        ValidationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Helper factory method to create an Error-severity issue.
    /// </summary>
    protected ValidationIssue CreateError(string message) =>
        new(RuleName, message, ValidationSeverity.Error);

    /// <summary>
    /// Helper factory method to create a Warning-severity issue.
    /// </summary>
    protected ValidationIssue CreateWarning(string message) =>
        new(RuleName, message, ValidationSeverity.Warning);

    /// <summary>
    /// Helper to return an empty issue list (rule passed).
    /// </summary>
    protected static readonly IReadOnlyList<ValidationIssue> NoIssues =
        Array.Empty<ValidationIssue>();
}

