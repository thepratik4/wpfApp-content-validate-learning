using NUnit.Framework;
using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.Tests.Unit;

[TestFixture]
public class ValidationSummaryTests
{
    [Test]
    public void Create_AccuratelyAggregatesMetricsAcrossResults()
    {
        // Arrange
        var file1 = new ContentFile("file1.pdf", @"C:\Course\file1.pdf", ".pdf", 100, DateTimeOffset.UtcNow);
        var file2 = new ContentFile("file2.mp4", @"C:\Course\file2.mp4", ".mp4", 100, DateTimeOffset.UtcNow);
        var file3 = new ContentFile("file3.json", @"C:\Course\file3.json", ".json", 100, DateTimeOffset.UtcNow);

        // Result 1: Perfectly valid (0 errors, 0 warnings)
        var res1 = ValidationResult.Success(file1);

        // Result 2: Warning only (0 errors, 2 warnings) -> Valid
        var res2 = new ValidationResult(file2, new[]
        {
            new ValidationIssue("Empty File Rule", "Empty file warning", ValidationSeverity.Warning),
            new ValidationIssue("Duplicate File Name Rule", "Duplicate warning", ValidationSeverity.Warning)
        });

        // Result 3: Has errors (2 errors, 1 warning) -> Invalid
        var res3 = new ValidationResult(file3, new[]
        {
            new ValidationIssue("Extension Rule", "Unsupported extension", ValidationSeverity.Error),
            new ValidationIssue("Required Metadata Rule", "Missing title", ValidationSeverity.Error),
            new ValidationIssue("Duplicate File Name Rule", "Duplicate warning", ValidationSeverity.Warning)
        });

        var results = new[] { res1, res2, res3 };
        var duration = TimeSpan.FromSeconds(2.4);

        // Act
        var summary = ValidationSummary.Create(results, duration);

        // Assert
        Assert.That(summary.TotalFiles, Is.EqualTo(3));
        Assert.That(summary.ValidFiles, Is.EqualTo(2), "res1 and res2 have 0 errors, so they count as Valid.");
        Assert.That(summary.InvalidFiles, Is.EqualTo(1), "res3 has errors, so it counts as Invalid.");
        Assert.That(summary.WarningCount, Is.EqualTo(3), "Total warnings across all files should be 3.");
        Assert.That(summary.ErrorCount, Is.EqualTo(2), "Total errors across all files should be 2.");
        Assert.That(summary.Duration, Is.EqualTo(duration));
        Assert.That(summary.FormattedDuration, Does.Contain("2.40 sec"));
    }

    [Test]
    public void Create_WhenNoResults_ReturnsZeroMetrics()
    {
        // Arrange
        var emptyResults = Array.Empty<ValidationResult>();

        // Act
        var summary = ValidationSummary.Create(emptyResults, TimeSpan.FromMilliseconds(250));

        // Assert
        Assert.That(summary.TotalFiles, Is.EqualTo(0));
        Assert.That(summary.ValidFiles, Is.EqualTo(0));
        Assert.That(summary.InvalidFiles, Is.EqualTo(0));
        Assert.That(summary.WarningCount, Is.EqualTo(0));
        Assert.That(summary.ErrorCount, Is.EqualTo(0));
        Assert.That(summary.FormattedDuration, Does.Contain("250 ms"));
    }
}

