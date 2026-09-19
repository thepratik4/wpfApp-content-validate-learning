using NUnit.Framework;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation;
using LearningContentValidator.Domain.Validation.Rules;
using LearningContentValidator.Tests.TestHelpers;

namespace LearningContentValidator.Tests.Unit;

[TestFixture]
public class RequiredMetadataRuleTests
{
    private RequiredMetadataRule _rule = null!;
    private ValidationOptions _options = null!;
    private TempDirectoryFixture _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _rule = new RequiredMetadataRule();
        _options = new ValidationOptions
        {
            RequiredMetadataFields = new List<string> { "id", "title", "version", "author" }
        };
        _tempDir = new TempDirectoryFixture();
    }

    [TearDown]
    public void TearDown()
    {
        _tempDir.Dispose();
    }

    [Test]
    public async Task ValidateAsync_WhenFileIsNotJson_ReturnsNoIssues()
    {
        // Arrange
        string filePath = _tempDir.CreateFile("course.pdf", "PDF content");
        var file = new ContentFile("course.pdf", filePath, ".pdf", 100, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Is.Empty);
    }

    [Test]
    public async Task ValidateAsync_WhenJsonContainsAllRequiredFields_ReturnsNoIssues()
    {
        // Arrange
        string json = """
        {
            "id": "COURSE-101",
            "title": "Introduction to 3D Design",
            "version": "1.0.0",
            "author": "Dassault Engineering"
        }
        """;
        string filePath = _tempDir.CreateFile("package.json", json);
        var file = new ContentFile("package.json", filePath, ".json", json.Length, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Is.Empty);
    }

    [Test]
    public async Task ValidateAsync_WhenJsonIsMissingRequiredField_ReturnsErrorIssue()
    {
        // Arrange (missing 'author')
        string json = """
        {
            "id": "COURSE-101",
            "title": "Introduction to 3D Design",
            "version": "1.0.0"
        }
        """;
        string filePath = _tempDir.CreateFile("package.json", json);
        var file = new ContentFile("package.json", filePath, ".json", json.Length, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Has.Count.EqualTo(1));
        Assert.That(issues[0].Severity, Is.EqualTo(ValidationSeverity.Error));
        Assert.That(issues[0].Message, Does.Contain("Missing mandatory metadata property 'author'"));
    }

    [Test]
    public async Task ValidateAsync_WhenRequiredFieldIsEmptyString_ReturnsErrorIssue()
    {
        // Arrange
        string json = """
        {
            "id": "COURSE-101",
            "title": "",
            "version": "1.0.0",
            "author": "Jane Doe"
        }
        """;
        string filePath = _tempDir.CreateFile("package.json", json);
        var file = new ContentFile("package.json", filePath, ".json", json.Length, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues.Any(i => i.Message.Contains("title") && i.Message.Contains("empty or null")), Is.True);
    }

    [Test]
    public async Task ValidateAsync_WhenJsonSyntaxIsMalformed_ReturnsErrorIssue()
    {
        // Arrange (broken JSON missing closing bracket)
        string malformed = "{ \"id\": \"COURSE-101\", \"title\": \"Broken\" ";
        string filePath = _tempDir.CreateFile("broken.json", malformed);
        var file = new ContentFile("broken.json", filePath, ".json", malformed.Length, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Has.Count.EqualTo(1));
        Assert.That(issues[0].Severity, Is.EqualTo(ValidationSeverity.Error));
        Assert.That(issues[0].Message, Does.Contain("Malformed JSON syntax"));
    }

    [Test]
    public async Task ValidateAsync_WhenJsonRootIsNotObject_ReturnsErrorIssue()
    {
        // Arrange (root is an array)
        string arrayJson = "[ { \"id\": \"COURSE-1\" }, { \"id\": \"COURSE-2\" } ]";
        string filePath = _tempDir.CreateFile("array.json", arrayJson);
        var file = new ContentFile("array.json", filePath, ".json", arrayJson.Length, DateTimeOffset.UtcNow);
        var context = new ValidationContext(_options, new[] { file });

        // Act
        var issues = await _rule.ValidateAsync(file, context);

        // Assert
        Assert.That(issues, Has.Count.EqualTo(1));
        Assert.That(issues[0].Message, Does.Contain("JSON root must be an object"));
    }
}

