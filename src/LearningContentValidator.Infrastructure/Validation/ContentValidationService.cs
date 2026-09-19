using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LearningContentValidator.Domain.Interfaces;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation;

namespace LearningContentValidator.Infrastructure.Validation;

/// <summary>
/// Orchestration engine that coordinates multi-rule validation over collections of content files.
/// Demonstrates Dependency Injection (constructor injection of IEnumerable&lt;IValidationRule&gt;),
/// asynchronous processing, cancellation, LINQ aggregation, and resilient error boundaries.
/// </summary>
public sealed class ContentValidationService : IContentValidationService
{
    private readonly IReadOnlyList<IValidationRule> _rules;
    private readonly ValidationOptions _defaultOptions;
    private readonly ILogger<ContentValidationService> _logger;

    public ContentValidationService(
        IEnumerable<IValidationRule> rules,
        IOptions<ValidationOptions> options,
        ILogger<ContentValidationService> logger)
    {
        _rules = rules?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(rules));
        _defaultOptions = options?.Value ?? new ValidationOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("ContentValidationService initialized with {RuleCount} rules registered.", _rules.Count);
    }

    public async Task<ValidationSessionResult> ValidateFilesAsync(
        IReadOnlyList<ContentFile> files,
        ValidationOptions? overrideOptions = null,
        IProgress<ValidationProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (files == null)
            throw new ArgumentNullException(nameof(files));

        var effectiveOptions = overrideOptions ?? _defaultOptions;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Beginning validation run on {FileCount} files with {RuleCount} rules.",
            files.Count, _rules.Count);

        // Precompute package context (e.g. filename lookup table) for O(1) cross-file queries
        var context = new ValidationContext(effectiveOptions, files);
        var results = new List<ValidationResult>(files.Count);

        for (int i = 0; i < files.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var file = files[i];
            var fileIssues = new List<ValidationIssue>();

            // Execute each registered rule in sequence for the current file
            foreach (var rule in _rules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var issues = await rule.ValidateAsync(file, context, cancellationToken)
                        .ConfigureAwait(false);

                    if (issues != null && issues.Count > 0)
                    {
                        fileIssues.AddRange(issues);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw; // Allow cancellation to propagate cleanly
                }
                catch (Exception ex)
                {
                    // Defensive boundary: an unhandled exception inside a single rule must not abort the entire run!
                    _logger.LogError(ex, "Unhandled exception while executing rule '{Rule}' on file '{File}'",
                        rule.RuleName, file.FileName);

                    fileIssues.Add(new ValidationIssue(
                        rule.RuleName,
                        $"Rule execution failed unexpectedly: {ex.Message}",
                        ValidationSeverity.Error));
                }
            }

            var result = new ValidationResult(file, fileIssues);
            results.Add(result);

            // Notify UI progress reporter
            progress?.Report(new ValidationProgressReport(
                current: i + 1,
                total: files.Count,
                currentFileName: file.FileName));
        }

        stopwatch.Stop();

        // Compile aggregate summary using LINQ to Objects
        var summary = ValidationSummary.Create(results, stopwatch.Elapsed);

        _logger.LogInformation(
            "Validation run completed in {Duration}. Total: {Total}, Valid: {Valid}, Invalid: {Invalid}, Errors: {Errors}, Warnings: {Warnings}",
            summary.FormattedDuration, summary.TotalFiles, summary.ValidFiles, summary.InvalidFiles, summary.ErrorCount, summary.WarningCount);

        return new ValidationSessionResult(results, summary, effectiveOptions);
    }
}

