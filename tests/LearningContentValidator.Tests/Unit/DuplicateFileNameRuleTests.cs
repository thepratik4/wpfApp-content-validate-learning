using NUnit.Framework;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation;
using LearningContentValidator.Domain.Validation.Rules;

namespace LearningContentValidator.Tests.Unit;

[TestFixture]
public class DuplicateFileNameRuleTests
{
    private DuplicateFileNameRule _rule = null!;
    private ValidationOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        _rule = new DuplicateFileNameRule();
        _options = new ValidationOptions { CheckDuplicateFileNames = true };
    }

    [Test]
    public async Task ValidateAsync_WhenFileNamesAreUnique_ReturnsNoIssues()
    {
        // Arrange
        var file1 = new ContentFile("intro.pdf", @"C:\Course\module1\intro.pdf", ".pdf", 100, DateTimeOffset.UtcNow);
        var file2 = new ContentFile("outro.pdf", @"C:\Course\module2\outro.pdf", ".pdf", 100, DateTimeOffset.UtcNow);
        var allFiles = new[] { file1, file2 };
        var context = new ValidationContext(_options, allFiles);

        // Act
        var issues1 = await _rule.ValidateAsync(file1, context);
        var issues2 = await _rule.ValidateAsync(file2, context);

        // Assert
        Assert.That(issues1, Is.Empty);
        Assert.That(issues2, Is.Empty);
    }

    [Test]
    public async Task ValidateAsync_WhenDuplicateFileNameExistsInAnotherDirectory_ReturnsWarningIssue()
    {
        // Arrange
        var file1 = new ContentFile("thumbnail.png", @"C:\Course\module1\thumbnail.png", ".png", 100, DateTimeOffset.UtcNow);
        var file2 = new ContentFile("thumbnail.png", @"C:\Course\module2\thumbnail.png", ".png", 100, DateTimeOffset.UtcNow);
        var allFiles = new[] { file1, file2 };
        var context = new ValidationContext(_options, allFiles);

        // Act
        var issues = await _rule.ValidateAsync(file1, context);

        // Assert
        Assert.That(issues, Has.Count.EqualTo(1));
        var issue = issues[0];
        Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Warning));
        Assert.That(issue.RuleName, Is.EqualTo("Duplicate File Name Rule"));
        Assert.That(issue.Message, Does.Contain("Duplicate file name detected"));
        Assert.That(issue.Message, Does.Contain(@"C:\Course\module2\thumbnail.png"));
    }

    [Test]
    public async Task ValidateAsync_WhenCheckDuplicateFileNamesIsDisabled_ReturnsNoIssues()
    {
        // Arrange
        _options.CheckDuplicateFileNames = false;
        var file1 = new ContentFile("dup.png", @"C:\Course\dir1\dup.png", ".png", 100, DateTimeOffset.UtcNow);
        var file2 = new ContentFile("dup.png", @"C:\Course\dir2\dup.png", ".png", 100, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file1, file2 });

        // Act
        var issues = await _rule.ValidateAsync(file1, context);

        // Assert
        Assert.That(issues, Is.Empty);
    }
}

