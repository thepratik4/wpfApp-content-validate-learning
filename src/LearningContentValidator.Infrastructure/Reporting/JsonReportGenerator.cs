using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using LearningContentValidator.Domain.Interfaces;
using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Infrastructure.Reporting;

/// <summary>
/// Exports validation results and summary metrics into an indented, human-readable JSON file.
/// Demonstrates System.Text.Json serialization, custom DTO projection, and async I/O.
/// </summary>
public sealed class JsonReportGenerator : IReportGenerator
{
    private readonly ILogger<JsonReportGenerator> _logger;

    public string SupportedFormat => "JSON";
    public string DefaultExtension => ".json";

    public JsonReportGenerator(ILogger<JsonReportGenerator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GenerateReportString(ValidationReport report)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        var dto = new
        {
            generatedAt = report.GeneratedAt.ToString("o"),
            scannedFolder = report.ScannedFolder,
            summary = new
            {
                totalFiles = report.Summary.TotalFiles,
                validFiles = report.Summary.ValidFiles,
                invalidFiles = report.Summary.InvalidFiles,
                warningCount = report.Summary.WarningCount,
                errorCount = report.Summary.ErrorCount,
                durationSeconds = report.Summary.Duration.TotalSeconds,
                formattedDuration = report.Summary.FormattedDuration
            },
            results = report.Results.Select(r => new
            {
                fileName = r.FileName,
                filePath = r.FilePath,
                fileType = r.File.FileType.ToString(),
                fileSizeBytes = r.File.FileSize,
                formattedSize = r.File.FormattedSize,
                status = r.StatusText,
                isValid = r.IsValid,
                errorCount = r.ErrorCount,
                warningCount = r.WarningCount,
                errors = r.Errors,
                warnings = r.Warnings,
                issues = r.Issues.Select(i => new
                {
                    ruleName = i.RuleName,
                    severity = i.Severity.ToString(),
                    message = i.Message
                })
            })
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        return JsonSerializer.Serialize(dto, options);
    }

    public async Task GenerateReportAsync(
        ValidationReport report,
        string destinationFilePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(destinationFilePath))
            throw new ArgumentException("Destination path cannot be null or empty.", nameof(destinationFilePath));

        _logger.LogInformation("Exporting JSON validation report to: {Path}", destinationFilePath);

        string json = GenerateReportString(report);

        string? directory = Path.GetDirectoryName(destinationFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(destinationFilePath, json, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("JSON validation report exported successfully ({Bytes} bytes).", json.Length);
    }
}

