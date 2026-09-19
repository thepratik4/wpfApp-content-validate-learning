using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using LearningContentValidator.Domain.Interfaces;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation;
using LearningContentValidator.Infrastructure.Validation;

namespace LearningContentValidator.Tests.Integration;

[TestFixture]
public class ContentValidationServiceTests
{
    private ValidationOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new ValidationOptions();
    }

    [Test]
    public async Task ValidateFilesAsync_ExecutesAllRulesAndAggregatesSummary()
    {
        // Arrange - Using Moq to simulate two independent validation rules
        var mockRule1 = new Mock<IValidationRule>();
        mockRule1.SetupGet(r => r.RuleName).Returns("Rule 1");
        mockRule1.Setup(r => r.ValidateAsync(It.IsAny<ContentFile>(), It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ValidationIssue>()); // Always passes

        var mockRule2 = new Mock<IValidationRule>();
        mockRule2.SetupGet(r => r.RuleName).Returns("Rule 2");
        mockRule2.Setup(r => r.ValidateAsync(It.IsAny<ContentFile>(), It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentFile f, ValidationContext c, CancellationToken ct) =>
                f.FileName.Contains("bad")
                    ? new[] { new ValidationIssue("Rule 2", "Flagged bad file", ValidationSeverity.Error) }
                    : Array.Empty<ValidationIssue>());

        var rules = new[] { mockRule1.Object, mockRule2.Object };
        var service = new ContentValidationService(
            rules,
            Options.Create(_options),
            NullLogger<ContentValidationService>.Instance);

        var file1 = new ContentFile("good.pdf", @"C:\Course\good.pdf", ".pdf", 100, DateTimeOffset.UtcNow);
        var file2 = new ContentFile("bad_file.pdf", @"C:\Course\bad_file.pdf", ".pdf", 100, DateTimeOffset.UtcNow);
        var files = new[] { file1, file2 };

        // Act
        var session = await service.ValidateFilesAsync(files);

        // Assert
        Assert.That(session.Results, Has.Count.EqualTo(2));
        Assert.That(session.Summary.TotalFiles, Is.EqualTo(2));
        Assert.That(session.Summary.ValidFiles, Is.EqualTo(1));
        Assert.That(session.Summary.InvalidFiles, Is.EqualTo(1));
        Assert.That(session.Summary.ErrorCount, Is.EqualTo(1));

        // Verify each mock rule was invoked exactly once per file
        mockRule1.Verify(r => r.ValidateAsync(It.IsAny<ContentFile>(), It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        mockRule2.Verify(r => r.ValidateAsync(It.IsAny<ContentFile>(), It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task ValidateFilesAsync_WhenRuleThrowsUnexpectedException_HandlesGracefullyWithoutFailingWholeRun()
    {
        // Arrange - A faulty rule that throws an unexpected exception
        var crashingRule = new Mock<IValidationRule>();
        crashingRule.SetupGet(r => r.RuleName).Returns("Faulty Rule");
        crashingRule.Setup(r => r.ValidateAsync(It.IsAny<ContentFile>(), It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated unexpected internal crash in rule!"));

        var service = new ContentValidationService(
            new[] { crashingRule.Object },
            Options.Create(_options),
            NullLogger<ContentValidationService>.Instance);

        var file = new ContentFile("test.pdf", @"C:\Course\test.pdf", ".pdf", 100, DateTimeOffset.UtcNow);

        // Act
        var session = await service.ValidateFilesAsync(new[] { file });

        // Assert: The service caught the exception, created an Error issue, and completed the session!
        Assert.That(session.Results, Has.Count.EqualTo(1));
        var result = session.Results[0];
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorCount, Is.EqualTo(1));
        Assert.That(result.Errors[0], Does.Contain("Rule execution failed unexpectedly"));
        Assert.That(session.Summary.ErrorCount, Is.EqualTo(1));
    }

    [Test]
    public void ValidateFilesAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockRule = new Mock<IValidationRule>();
        var service = new ContentValidationService(
            new[] { mockRule.Object },
            Options.Create(_options),
            NullLogger<ContentValidationService>.Instance);

        var file = new ContentFile("test.pdf", @"C:\Course\test.pdf", ".pdf", 100, DateTimeOffset.UtcNow);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await service.ValidateFilesAsync(new[] { file }, cancellationToken: cts.Token));
    }
}

