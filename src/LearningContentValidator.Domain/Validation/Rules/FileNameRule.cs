using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Validation.Rules;

/// <summary>
/// Rule 4: File Name Rule
/// Validates that file names adhere to cross-platform safe conventions:
/// - No invalid filesystem characters
/// - No reserved Windows device names (CON, PRN, AUX, NUL, COM1-9, LPT1-9)
/// - No leading or trailing whitespace
/// - Respects maximum filename length limits
/// Demonstrates LINQ Any, string manipulation, and defensive programming.
/// </summary>
public sealed class FileNameRule : ValidationRuleBase
{
    public override string RuleName => "File Name Rule";
    public override string Description => "Detects invalid characters, trailing whitespace, and reserved names in file names.";

    private static readonly HashSet<string> ReservedDeviceNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public override Task<IReadOnlyList<ValidationIssue>> ValidateAsync(
        ContentFile file,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<ValidationIssue>();
        string fileName = file.FileName;
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        // Check 1: Exceeding maximum allowed length
        if (fileName.Length > context.Options.MaxFileNameLength)
        {
            issues.Add(CreateError(
                $"File name exceeds maximum permitted length of {context.Options.MaxFileNameLength} characters (actual: {fileName.Length})."));
        }

        // Check 2: Leading or trailing whitespace
        if (fileName != fileName.Trim())
        {
            issues.Add(CreateError("File name contains leading or trailing whitespace."));
        }

        // Check 3: Invalid characters
        char[] invalidChars = context.Options.DisallowedFilenameCharacters;
        if (fileName.IndexOfAny(invalidChars) >= 0)
        {
            // Demonstrates LINQ Where and Distinct to list the specific invalid characters encountered
            var found = fileName.Where(c => invalidChars.Contains(c)).Distinct();
            string charList = string.Join(" ", found.Select(c => $"'{c}'"));
            issues.Add(CreateError($"File name contains forbidden characters: {charList}."));
        }

        // Check 4: Reserved device names
        if (ReservedDeviceNames.Contains(nameWithoutExtension))
        {
            issues.Add(CreateError(
                $"File name '{nameWithoutExtension}' is a reserved Windows system device name."));
        }

        return Task.FromResult<IReadOnlyList<ValidationIssue>>(issues);
    }
}

