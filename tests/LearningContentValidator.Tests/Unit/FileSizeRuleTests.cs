using NUnit.Framework;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation;
using LearningContentValidator.Domain.Validation.Rules;

namespace LearningContentValidator.Tests.Unit;

[TestFixture]
public class FileSizeRuleTests
{
    private FileSizeRule _rule = null!;
    private ValidationOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        _rule = new FileSizeRule();
        _options = new ValidationOptions
        {
            MaxFileSizeInBytes = 10 * 1024 * 1024 // 10 MB limit for test
        };
    }

    [Test]
    public async Task ValidateAsync_WhenFileSizeIsUnderLimit_ReturnsNoIssues()
    {
        // Arrange
        var file = new ContentFile("manual.pdf", @"C:\Course\manual.pdf", ".pdf", 5 * 1024 * 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Is.Empty);
    }

    [Test]
    public async Task ValidateAsync_WhenFileSizeEqualsLimit_ReturnsNoIssues()
    {
        // Arrange
        var file = new ContentFile("exact.pdf", @"C:\Course\exact.pdf", ".pdf", 10 * 1024 * 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Is.Empty);
    }

    [Test]
    public async Task ValidateAsync_WhenFileSizeExceedsLimit_ReturnsErrorIssue()
    {
        // Arrange
        var file = new ContentFile("huge_video.mp4", @"C:\Course\huge_video.mp4", ".mp4", 50 * 1024 * 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Has.Count.EqualTo(1));
        var issue = issues[0];
        Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error));
        Assert.That(issue.RuleName, Is.EqualTo("File Size Rule"));
        Assert.That(issue.Message, Does.Contain("exceeds maximum allowed limit"));
    }
}

