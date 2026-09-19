namespace LearningContentValidator.Domain.Models;

/// <summary>
/// Severity level of an individual validation outcome or violation.
/// </summary>
public enum ValidationSeverity
{
    Information = 0,
    Warning = 1,
    Error = 2
}

