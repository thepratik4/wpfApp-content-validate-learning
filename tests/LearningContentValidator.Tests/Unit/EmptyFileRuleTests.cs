using NUnit.Framework;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation;
using LearningContentValidator.Domain.Validation.Rules;

namespace LearningContentValidator.Tests.Unit;

[TestFixture]
public class EmptyFileRuleTests
{
    private EmptyFileRule _rule = null!;
    private ValidationOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        _rule = new EmptyFileRule();
        _options = new ValidationOptions { CheckZeroByteFiles = true };
    }

    [Test]
    public async Task ValidateAsync_WhenFileHasContent_ReturnsNoIssues()
    {
        // Arrange
        var file = new ContentFile("valid.pdf", @"C:\Course\valid.pdf", ".pdf", 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Is.Empty);
    }

    [Test]
    public async Task ValidateAsync_WhenFileIsZeroBytes_ReturnsWarningIssue()
    {
        // Arrange
        var file = new ContentFile("placeholder.mp4", @"C:\Course\placeholder.mp4", ".mp4", 0, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Has.Count.EqualTo(1));
        var issue = issues[0];
        Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Warning));
        Assert.That(issue.RuleName, Is.EqualTo("Empty File Rule"));
        Assert.That(issue.Message, Does.Contain("0 bytes"));
    }

    [Test]
    public async Task ValidateAsync_WhenCheckZeroByteFilesIsDisabled_ReturnsNoIssues()
    {
        // Arrange
        _options.CheckZeroByteFiles = false;
        var file = new ContentFile("empty.txt", @"C:\Course\empty.txt", ".txt", 0, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Is.Empty);
    }
}

