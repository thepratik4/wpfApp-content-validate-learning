using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Validation.Rules;

/// <summary>
/// Rule 3: Empty File Rule
/// Detects 0-byte files which typically indicate truncated downloads, broken placeholders, or corrupted assets.
/// Flags as a Warning so users can investigate without necessarily blocking publishing if intentional.
/// </summary>
public sealed class EmptyFileRule : ValidationRuleBase
{
    public override string RuleName => "Empty File Rule";
    public override string Description => "Detects zero-byte files in the package.";

    public override Task<IReadOnlyList<ValidationIssue>> ValidateAsync(
        ContentFile file,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.Options.CheckZeroByteFiles)
            return Task.FromResult(NoIssues);

        if (file.FileSize == 0)
        {
            var issue = CreateWarning(
                $"File '{file.FileName}' has a size of 0 bytes (empty file), which may indicate corrupted or incomplete content.");
            return Task.FromResult<IReadOnlyList<ValidationIssue>>(new[] { issue });
        }

        return Task.FromResult(NoIssues);
    }
}

