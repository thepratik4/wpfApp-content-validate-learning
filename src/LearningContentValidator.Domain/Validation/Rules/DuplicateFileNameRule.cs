using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Validation.Rules;

/// <summary>
/// Rule 6: Duplicate File Name Rule
/// Detects multiple files across the course package sharing the same file name (in different directories).
/// In learning management systems (LMS) or offline SCORM packages, duplicate asset names frequently cause
/// overwrite collisions or broken relative links during package extraction.
/// Demonstrates O(1) multi-file querying using the pre-indexed ValidationContext.FilesByName lookup.
/// </summary>
public sealed class DuplicateFileNameRule : ValidationRuleBase
{
    public override string RuleName => "Duplicate File Name Rule";
    public override string Description => "Detects duplicate file names across different directories in the package.";

    public override Task<IReadOnlyList<ValidationIssue>> ValidateAsync(
        ContentFile file,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.Options.CheckDuplicateFileNames)
            return Task.FromResult(NoIssues);

        // O(1) lookup into the pre-computed LINQ ILookup on the ValidationContext
        var duplicates = context.FilesByName[file.FileName]
            .Where(f => !string.Equals(f.FullPath, file.FullPath, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (duplicates.Count > 0)
        {
            string duplicatePaths = string.Join("; ", duplicates.Select(d => d.FullPath));
            var issue = CreateWarning(
                $"Duplicate file name detected. '{file.FileName}' also exists at: {duplicatePaths}.");
            return Task.FromResult<IReadOnlyList<ValidationIssue>>(new[] { issue });
        }

        return Task.FromResult(NoIssues);
    }
}

