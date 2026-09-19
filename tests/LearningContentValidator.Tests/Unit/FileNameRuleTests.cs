using NUnit.Framework;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation;
using LearningContentValidator.Domain.Validation.Rules;

namespace LearningContentValidator.Tests.Unit;

[TestFixture]
public class FileNameRuleTests
{
    private FileNameRule _rule = null!;
    private ValidationOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        _rule = new FileNameRule();
        _options = new ValidationOptions();
    }

    [Test]
    public async Task ValidateAsync_WhenFileNameIsValid_ReturnsNoIssues()
    {
        // Arrange
        var file = new ContentFile("Lesson-01_Introduction.pdf", @"C:\Course\Lesson-01_Introduction.pdf", ".pdf", 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Is.Empty);
    }

    [TestCase(" trailing_space.pdf ")]
    [TestCase(" leading_space.pdf")]
    [TestCase("ends_with_space.pdf ")]
    public async Task ValidateAsync_WhenFileNameHasLeadingOrTrailingWhitespace_ReturnsErrorIssue(string invalidName)
    {
        // Arrange
        var file = new ContentFile(invalidName, @"C:\Course\" + invalidName, ".pdf", 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(issues.Any(i => i.Message.Contains("whitespace")), Is.True);
    }

    [TestCase("bad:name.pdf")]
    [TestCase("bad*name.pdf")]
    [TestCase("bad?name.pdf")]
    [TestCase("bad\"name.pdf")]
    [TestCase("bad<name.pdf")]
    [TestCase("bad>name.pdf")]
    [TestCase("bad|name.pdf")]
    public async Task ValidateAsync_WhenFileNameContainsIllegalChars_ReturnsErrorIssue(string invalidName)
    {
        // Arrange
        var file = new ContentFile(invalidName, @"C:\Course\" + invalidName, ".pdf", 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues.Any(i => i.Message.Contains("forbidden characters")), Is.True);
    }

    [TestCase("CON.json")]
    [TestCase("aux.pdf")]
    [TestCase("NUL.html")]
    [TestCase("COM1.png")]
    [TestCase("LPT1.zip")]
    public async Task ValidateAsync_WhenFileNameIsReservedDeviceName_ReturnsErrorIssue(string reservedName)
    {
        // Arrange
        var file = new ContentFile(reservedName, @"C:\Course\" + reservedName, Path.GetExtension(reservedName), 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues.Any(i => i.Message.Contains("reserved Windows system device name")), Is.True);
    }

    [Test]
    public async Task ValidateAsync_WhenFileNameExceedsMaxLength_ReturnsErrorIssue()
    {
        // Arrange
        string longName = new string('a', 260) + ".pdf";
        var file = new ContentFile(longName, @"C:\Course\" + longName, ".pdf", 1024, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues.Any(i => i.Message.Contains("maximum permitted length")), Is.True);
    }
}

