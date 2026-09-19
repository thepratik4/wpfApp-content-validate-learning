using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Domain.Interfaces;

/// <summary>
/// Abstraction for exporting validation reports to distinct file formats (JSON, CSV, etc.).
/// Demonstrates the Strategy Pattern, allowing export formats to be interchanged or expanded.
/// </summary>
public interface IReportGenerator
{
    /// <summary>
    /// Format identifier (e.g., "JSON", "CSV").
    /// </summary>
    string SupportedFormat { get; }

    /// <summary>
    /// Standard file extension for this format (e.g., ".json", ".csv").
    /// </summary>
    string DefaultExtension { get; }

    /// <summary>
    /// Generates the report content as an in-memory formatted string.
    /// </summary>
    string GenerateReportString(ValidationReport report);

    /// <summary>
    /// Asynchronously writes the formatted report to a destination file on disk.
    /// </summary>
    Task GenerateReportAsync(
        ValidationReport report,
        string destinationFilePath,
        CancellationToken cancellationToken = default);
}

