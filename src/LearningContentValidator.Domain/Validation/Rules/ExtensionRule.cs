using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Validation.Rules;

/// <summary>
/// Rule 2: Extension Rule
/// Flags files with unapproved or unsupported file extensions.
/// Demonstrates O(1) membership lookup using HashSet.Contains.
/// </summary>
public sealed class ExtensionRule : ValidationRuleBase
{
    public override string RuleName => "Extension Rule";
    public override string Description => "Rejects files whose extensions are not whitelisted.";

    public override Task<IReadOnlyList<ValidationIssue>> ValidateAsync(
        ContentFile file,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        string extension = file.Extension;

        if (string.IsNullOrWhiteSpace(extension))
        {
            var issue = CreateError("File has no extension and cannot be recognized as a valid package asset.");
            return Task.FromResult<IReadOnlyList<ValidationIssue>>(new[] { issue });
        }

        if (!context.Options.AllowedExtensions.Contains(extension))
        {
            string allowedList = string.Join(", ", context.Options.AllowedExtensions);
            var issue = CreateError(
                $"File extension '{extension}' is not supported. Supported extensions are: {allowedList}.");
            return Task.FromResult<IReadOnlyList<ValidationIssue>>(new[] { issue });
        }

        return Task.FromResult(NoIssues);
    }
}

