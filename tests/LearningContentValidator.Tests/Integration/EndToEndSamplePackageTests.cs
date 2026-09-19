using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using LearningContentValidator.Domain.Interfaces;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation.Rules;
using LearningContentValidator.Infrastructure.FileSystem;
using LearningContentValidator.Infrastructure.Reporting;
using LearningContentValidator.Infrastructure.Validation;

namespace LearningContentValidator.Tests.Integration;

[TestFixture]
public class EndToEndSamplePackageTests
{
    private string _sampleFolderPath = null!;

    [SetUp]
    public void SetUp()
    {
        // Resolve path to the bundled samples directory
        string baseDir = TestContext.CurrentContext.TestDirectory;
        // Traverse up to find the repository root containing 'samples'
        var dir = new DirectoryInfo(baseDir);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "samples")))
        {
            dir = dir.Parent;
        }

        if (dir == null)
        {
            Assert.Inconclusive("Samples directory not found from test runner context.");
            return;
        }

        _sampleFolderPath = Path.Combine(dir.FullName, "samples", "CoursePackage_Physics101");
    }

    [Test]
    public async Task ValidateSamplePackage_ScansValidatesAndGeneratesReportsSuccessfully()
    {
        // 1. Scan directory
        var scanner = new FileScanner(NullLogger<FileScanner>.Instance);
        var files = scanner.Scan(_sampleFolderPath);

        Assert.That(files, Has.Count.GreaterThanOrEqualTo(8), "Sample package should contain at least 8 test assets.");

        // 2. Setup Validation Engine with all 6 production rules
        var options = new ValidationOptions();
        var rules = new IValidationRule[]
        {
            new FileSizeRule(),
            new ExtensionRule(),
            new EmptyFileRule(),
            new FileNameRule(),
            new RequiredMetadataRule(),
            new DuplicateFileNameRule()
        };

        var service = new ContentValidationService(
            rules,
            Options.Create(options),
            NullLogger<ContentValidationService>.Instance);

        // 3. Execute validation
        var session = await service.ValidateFilesAsync(files);

        // Assert: Ensure metrics reflect the expected sample package state
        Assert.That(session.Summary.TotalFiles, Is.EqualTo(files.Count));
        Assert.That(session.Summary.ErrorCount, Is.GreaterThan(0), "Sample package should contain deliberate error files (bad_script.bat, broken_meta.json).");
        Assert.That(session.Summary.WarningCount, Is.GreaterThan(0), "Sample package should contain deliberate warning files (corrupted_empty.mp4, duplicate syllabus.pdf).");
        Assert.That(session.Summary.ValidFiles, Is.GreaterThan(0), "Sample package should contain valid files (syllabus.pdf, course_manifest.json, etc.).");

        // 4. Test Report Generation
        var report = new ValidationReport(_sampleFolderPath, session.Summary, session.Results, options);
        var jsonGen = new JsonReportGenerator(NullLogger<JsonReportGenerator>.Instance);
        var csvGen = new CsvReportGenerator(NullLogger<CsvReportGenerator>.Instance);

        string jsonOutput = jsonGen.GenerateReportString(report);
        string csvOutput = csvGen.GenerateReportString(report);

        Assert.That(jsonOutput, Does.Contain("course_manifest.json"));
        Assert.That(jsonOutput, Does.Contain("bad_script.bat"));
        Assert.That(csvOutput, Does.Contain("syllabus.pdf"));
    }
}
