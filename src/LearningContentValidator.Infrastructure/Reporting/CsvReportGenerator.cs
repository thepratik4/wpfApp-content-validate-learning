using System.Text;
using Microsoft.Extensions.Logging;
using LearningContentValidator.Domain.Interfaces;
using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Infrastructure.Reporting;

/// <summary>
/// Exports validation results into an RFC 4180 compliant Comma-Separated Values (CSV) file.
/// Demonstrates robust CSV string escaping, StringBuilder efficiency, and async file writing.
/// </summary>
public sealed class CsvReportGenerator : IReportGenerator
{
    private readonly ILogger<CsvReportGenerator> _logger;

    public string SupportedFormat => "CSV";
    public string DefaultExtension => ".csv";

    public CsvReportGenerator(ILogger<CsvReportGenerator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GenerateReportString(ValidationReport report)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        var sb = new StringBuilder();

        // 1. Metadata / Summary Header Section
        sb.AppendLine("# Learning Content Validator Report");
        sb.AppendLine($"# Generated At: {report.GeneratedAt:u}");
        sb.AppendLine($"# Scanned Folder: {EscapeCsv(report.ScannedFolder)}");
        sb.AppendLine($"# Total Files: {report.Summary.TotalFiles}, Valid: {report.Summary.ValidFiles}, Invalid: {report.Summary.InvalidFiles}, Warnings: {report.Summary.WarningCount}, Errors: {report.Summary.ErrorCount}");
        sb.AppendLine($"# Duration: {report.Summary.FormattedDuration}");
        sb.AppendLine();

        // 2. CSV Table Column Headers
        sb.AppendLine("FileName,FileType,FileSize,Status,IsValid,ErrorCount,WarningCount,Errors,Warnings,FilePath");

        // 3. Rows
        foreach (var result in report.Results)
        {
            string errorsJoined = string.Join(" | ", result.Errors);
            string warningsJoined = string.Join(" | ", result.Warnings);

            sb.Append(EscapeCsv(result.FileName)).Append(',');
            sb.Append(EscapeCsv(result.File.FileType.ToString())).Append(',');
            sb.Append(result.File.FileSize).Append(',');
            sb.Append(EscapeCsv(result.StatusText)).Append(',');
            sb.Append(result.IsValid ? "true" : "false").Append(',');
            sb.Append(result.ErrorCount).Append(',');
            sb.Append(result.WarningCount).Append(',');
            sb.Append(EscapeCsv(errorsJoined)).Append(',');
            sb.Append(EscapeCsv(warningsJoined)).Append(',');
            sb.Append(EscapeCsv(result.FilePath));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public async Task GenerateReportAsync(
        ValidationReport report,
        string destinationFilePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(destinationFilePath))
            throw new ArgumentException("Destination path cannot be null or empty.", nameof(destinationFilePath));

        _logger.LogInformation("Exporting CSV validation report to: {Path}", destinationFilePath);

        string csv = GenerateReportString(report);

        string? directory = Path.GetDirectoryName(destinationFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(destinationFilePath, csv, Encoding.UTF8, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("CSV validation report exported successfully ({Bytes} bytes).", csv.Length);
    }

    /// <summary>
    /// Escapes CSV field value according to RFC 4180:
    /// Values containing commas, double-quotes, or newlines must be enclosed in quotes,
    /// and any inner double-quotes doubled.
    /// </summary>
    public static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        bool mustQuote = value.Contains(',') ||
                         value.Contains('"') ||
                         value.Contains('\n') ||
                         value.Contains('\r');

        if (!mustQuote)
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}

