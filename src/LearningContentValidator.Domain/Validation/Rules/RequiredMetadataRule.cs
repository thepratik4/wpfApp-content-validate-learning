using System.Text.Json;
using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Validation.Rules;

/// <summary>
/// Rule 5: Required Metadata Rule
/// Inspects JSON metadata files (e.g. manifest.json, package.json, course.json) to verify:
/// 1. File contains valid, well-formed JSON syntax.
/// 2. Root element is a JSON object.
/// 3. All configured mandatory fields (id, title, version, author, etc.) are present and non-empty.
/// Demonstrates async I/O, System.Text.Json, exception handling, and cancellation.
/// </summary>
public sealed class RequiredMetadataRule : ValidationRuleBase
{
    public override string RuleName => "Required Metadata Rule";
    public override string Description => "Validates JSON metadata structure and mandatory schema fields.";

    public override async Task<IReadOnlyList<ValidationIssue>> ValidateAsync(
        ContentFile file,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        // Only inspect JSON files
        if (file.FileType != ContentFileType.Json)
            return NoIssues;

        // Skip 0-byte JSON files (EmptyFileRule already flags them)
        if (file.FileSize == 0)
            return NoIssues;

        var issues = new List<ValidationIssue>();

        try
        {
            // Defensive check: ensure file exists before reading
            if (!File.Exists(file.FullPath))
            {
                issues.Add(CreateError($"File not found on disk at: '{file.FullPath}'."));
                return issues;
            }

            // Asynchronous file read respecting cooperative cancellation
            string jsonText = await File.ReadAllTextAsync(file.FullPath, cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                issues.Add(CreateWarning("JSON file contains only whitespace."));
                return issues;
            }

            using var doc = JsonDocument.Parse(jsonText);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                issues.Add(CreateError("JSON root must be an object (not an array or scalar value)."));
                return issues;
            }

            var root = doc.RootElement;
            foreach (string requiredField in context.Options.RequiredMetadataFields)
            {
                // Check if field exists in root object (case-insensitive search)
                bool found = false;
                foreach (var property in root.EnumerateObject())
                {
                    if (string.Equals(property.Name, requiredField, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        if (property.Value.ValueKind == JsonValueKind.Null ||
                            property.Value.ValueKind == JsonValueKind.Undefined ||
                            (property.Value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(property.Value.GetString())))
                        {
                            issues.Add(CreateError(
                                $"Required metadata property '{requiredField}' exists but is empty or null."));
                        }
                        break;
                    }
                }

                if (!found)
                {
                    issues.Add(CreateError(
                        $"Missing mandatory metadata property '{requiredField}' in JSON file."));
                }
            }
        }
        catch (JsonException ex)
        {
            // Specifically catch malformed JSON without crashing the validation run
            issues.Add(CreateError($"Malformed JSON syntax: {ex.Message}"));
        }
        catch (IOException ex)
        {
            // File lock or access error handled gracefully
            issues.Add(CreateError($"Could not read metadata file due to I/O error: {ex.Message}"));
        }
        catch (UnauthorizedAccessException ex)
        {
            issues.Add(CreateError($"Access denied when attempting to read file: {ex.Message}"));
        }

        return issues;
    }
}

