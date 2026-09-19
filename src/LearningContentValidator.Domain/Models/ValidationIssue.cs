namespace LearningContentValidator.Domain.Models;

/// <summary>
/// Represents an individual validation issue (warning or error) flagged against a content file.
/// Demonstrates immutability, encapsulation, and readonly properties.
/// </summary>
public sealed class ValidationIssue
{
    public string RuleName { get; }
    public string Message { get; }
    public ValidationSeverity Severity { get; }

    public ValidationIssue(string ruleName, string message, ValidationSeverity severity)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name cannot be null or whitespace.", nameof(ruleName));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));

        RuleName = ruleName;
        Message = message;
        Severity = severity;
    }

    public override string ToString() => $"[{Severity}] ({RuleName}) {Message}";
}

