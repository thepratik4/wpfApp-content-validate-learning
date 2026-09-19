using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Validation.Rules;

/// <summary>
/// Rule 1: File Size Rule
/// Flags files that exceed the maximum allowed size configured in ValidationOptions.
/// </summary>
public sealed class FileSizeRule : ValidationRuleBase
{
    public override string RuleName => "File Size Rule";
    public override string Description => "Rejects files that exceed the maximum allowed package file size.";

    public override Task<IReadOnlyList<ValidationIssue>> ValidateAsync(
        ContentFile file,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        long limit = context.Options.MaxFileSizeInBytes;

        if (file.FileSize > limit)
        {
            var issue = CreateError(
                $"File size of {file.FormattedSize} exceeds maximum allowed limit of {ContentFile.FormatByteSize(limit)}.");
            return Task.FromResult<IReadOnlyList<ValidationIssue>>(new[] { issue });
        }

        return Task.FromResult(NoIssues);
    }
}

