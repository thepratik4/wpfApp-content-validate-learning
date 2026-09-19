using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using LearningContentValidator.Domain.Interfaces;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Infrastructure.Reporting;
using LearningContentValidator.Tests.TestHelpers;

namespace LearningContentValidator.Tests.Unit;

[TestFixture]
public class ReportGeneratorTests
{
    private TempDirectoryFixture _tempDir = null!;
    private ValidationReport _sampleReport = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = new TempDirectoryFixture();

        var file1 = new ContentFile("intro.pdf", @"C:\Course\intro.pdf", ".pdf", 2048, DateTimeOffset.UtcNow);
        var file2 = new ContentFile("video.mp4", @"C:\Course\video.mp4", ".mp4", 10000, DateTimeOffset.UtcNow);

        var res1 = ValidationResult.Success(file1);
        var res2 = new ValidationResult(file2, new[]
        {
            new ValidationIssue("File Size Rule", "File exceeds configured maximum", ValidationSeverity.Error),
            new ValidationIssue("Duplicate File Name Rule", "Possible duplicate", ValidationSeverity.Warning)
        });

        var results = new[] { res1, res2 };
        var summary = ValidationSummary.Create(results, TimeSpan.FromSeconds(1.5));

        _sampleReport = new ValidationReport(@"C:\Course", summary, results, new ValidationOptions());
    }

    [TearDown]
    public void TearDown()
    {
        _tempDir.Dispose();
    }

    [Test]
    public async Task JsonReportGenerator_GeneratesValidJsonFileWithAllDetails()
    {
        // Arrange
        var generator = new JsonReportGenerator(NullLogger<JsonReportGenerator>.Instance);
        string outputPath = Path.Combine(_tempDir.DirectoryPath, "report.json");

        // Act
        await generator.GenerateReportAsync(_sampleReport, outputPath);

        // Assert
        Assert.That(File.Exists(outputPath), Is.True);
        string json = await File.ReadAllTextAsync(outputPath);
        Assert.That(json, Is.Not.Empty);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.That(root.GetProperty("scannedFolder").GetString(), Is.EqualTo(@"C:\Course"));
        Assert.That(root.GetProperty("summary").GetProperty("totalFiles").GetInt32(), Is.EqualTo(2));
        Assert.That(root.GetProperty("summary").GetProperty("validFiles").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("summary").GetProperty("invalidFiles").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("results").GetArrayLength(), Is.EqualTo(2));
    }

    [Test]
    public async Task CsvReportGenerator_GeneratesValidCsvFileWithEscapedColumns()
    {
        // Arrange
        var generator = new CsvReportGenerator(NullLogger<CsvReportGenerator>.Instance);
        string outputPath = Path.Combine(_tempDir.DirectoryPath, "report.csv");

        // Act
        await generator.GenerateReportAsync(_sampleReport, outputPath);

        // Assert
        Assert.That(File.Exists(outputPath), Is.True);
        string[] lines = await File.ReadAllLinesAsync(outputPath);

        // Header comments and column header
        Assert.That(lines.Any(l => l.StartsWith("# Learning Content Validator Report")), Is.True);
        Assert.That(lines.Any(l => l.StartsWith("FileName,FileType,FileSize,Status")), Is.True);

        // Verify data rows
        Assert.That(lines.Any(l => l.Contains("intro.pdf") && l.Contains("Valid")), Is.True);
        Assert.That(lines.Any(l => l.Contains("video.mp4") && l.Contains("Invalid")), Is.True);
    }

    [Test]
    public void CsvReportGenerator_EscapeCsv_ProperlyEscapesCommasAndQuotes()
    {
        // Assert
        Assert.That(CsvReportGenerator.EscapeCsv("NormalText"), Is.EqualTo("NormalText"));
        Assert.That(CsvReportGenerator.EscapeCsv("Text,With,Commas"), Is.EqualTo("\"Text,With,Commas\""));
        Assert.That(CsvReportGenerator.EscapeCsv("TextWith\"Quote"), Is.EqualTo("\"TextWith\"\"Quote\""));
    }

    [Test]
    public void ReportGeneratorFactory_ResolvesRegisteredGenerators()
    {
        // Arrange
        var jsonGen = new JsonReportGenerator(NullLogger<JsonReportGenerator>.Instance);
        var csvGen = new CsvReportGenerator(NullLogger<CsvReportGenerator>.Instance);
        var factory = new ReportGeneratorFactory(new IReportGenerator[] { jsonGen, csvGen });

        // Act & Assert
        Assert.That(factory.GetByFormat("JSON"), Is.SameAs(jsonGen));
        Assert.That(factory.GetByFormat("CSV"), Is.SameAs(csvGen));
        Assert.That(factory.GetByExtension(".json"), Is.SameAs(jsonGen));
        Assert.That(factory.GetByExtension("csv"), Is.SameAs(csvGen));
        Assert.Throws<NotSupportedException>(() => factory.GetByFormat("XML"));
    }
}
