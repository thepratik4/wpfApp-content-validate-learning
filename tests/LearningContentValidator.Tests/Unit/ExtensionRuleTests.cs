using NUnit.Framework;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation;
using LearningContentValidator.Domain.Validation.Rules;

namespace LearningContentValidator.Tests.Unit;

[TestFixture]
public class ExtensionRuleTests
{
    private ExtensionRule _rule = null!;
    private ValidationOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        _rule = new ExtensionRule();
        _options = new ValidationOptions();
    }

    [TestCase(".pdf")]
    [TestCase(".PDF")]
    [TestCase(".json")]
    [TestCase(".mp4")]
    [TestCase(".zip")]
    public async Task ValidateAsync_WhenExtensionIsAllowed_ReturnsNoIssues(string extension)
    {
        // Arrange
        var file = new ContentFile("test" + extension, @"C:\Course\test" + extension, extension, 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Is.Empty);
    }

    [TestCase(".exe")]
    [TestCase(".dll")]
    [TestCase(".bat")]
    [TestCase(".sh")]
    public async Task ValidateAsync_WhenExtensionIsNotAllowed_ReturnsErrorIssue(string extension)
    {
        // Arrange
        var file = new ContentFile("malicious" + extension, @"C:\Course\malicious" + extension, extension, 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Has.Count.EqualTo(1));
        var issue = issues[0];
        Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error));
        Assert.That(issue.RuleName, Is.EqualTo("Extension Rule"));
        Assert.That(issue.Message, Does.Contain("not supported"));
    }

    [Test]
    public async Task ValidateAsync_WhenFileHasNoExtension_ReturnsErrorIssue()
    {
        // Arrange
        var file = new ContentFile("README", @"C:\Course\README", "", 500, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Has.Count.EqualTo(1));
        Assert.That(issues[0].Severity, Is.EqualTo(ValidationSeverity.Error));
        Assert.That(issues[0].Message, Does.Contain("has no extension"));
    }
}

