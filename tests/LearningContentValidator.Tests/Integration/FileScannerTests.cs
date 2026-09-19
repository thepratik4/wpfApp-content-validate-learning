using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Infrastructure.FileSystem;
using LearningContentValidator.Tests.TestHelpers;

namespace LearningContentValidator.Tests.Integration;

[TestFixture]
public class FileScannerTests
{
    private TempDirectoryFixture _tempDir = null!;
    private FileScanner _scanner = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = new TempDirectoryFixture();
        _scanner = new FileScanner(NullLogger<FileScanner>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _tempDir.Dispose();
    }

    [Test]
    public void Scan_RecursivelyFindsFilesInNestedDirectories()
    {
        // Arrange
        _tempDir.CreateFile("root_document.pdf", "PDF Content");
        _tempDir.CreateFile(@"module1\intro.mp4", "Video content");
        _tempDir.CreateFile(@"module1\lesson1\overview.html", "<html></html>");
        _tempDir.CreateFile(@"module2\quiz.json", "{ \"id\": 1 }");

        // Act
        var results = _scanner.Scan(_tempDir.DirectoryPath);

        // Assert
        Assert.That(results, Has.Count.EqualTo(4));
        Assert.That(results.Any(f => f.FileName == "root_document.pdf" && f.FileType == ContentFileType.Pdf), Is.True);
        Assert.That(results.Any(f => f.FileName == "intro.mp4" && f.FileType == ContentFileType.Video), Is.True);
        Assert.That(results.Any(f => f.FileName == "overview.html" && f.FileType == ContentFileType.Html), Is.True);
        Assert.That(results.Any(f => f.FileName == "quiz.json" && f.FileType == ContentFileType.Json), Is.True);
    }

    [Test]
    public void Scan_WhenDirectoryIsEmpty_ReturnsEmptyCollection()
    {
        // Act
        var results = _scanner.Scan(_tempDir.DirectoryPath);

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Scan_SkipsIgnoredDirectoriesAndSystemFiles()
    {
        // Arrange
        _tempDir.CreateFile("valid.pdf", "pdf");
        _tempDir.CreateFile(@".git\HEAD", "ref: refs/heads/main");
        _tempDir.CreateFile(@".vs\config.json", "{}");
        _tempDir.CreateFile(@"bin\Debug\app.dll", "assembly");
        _tempDir.CreateFile("desktop.ini", "desktop config");
        _tempDir.CreateFile("Thumbs.db", "thumbnail cache");
        _tempDir.CreateFile("~$presentation.pptx", "lock file");

        // Act
        var results = _scanner.Scan(_tempDir.DirectoryPath);

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].FileName, Is.EqualTo("valid.pdf"));
    }

    [Test]
    public void Scan_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        string nonexistent = Path.Combine(_tempDir.DirectoryPath, "DoesNotExist_" + Guid.NewGuid().ToString("N"));

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => _scanner.Scan(nonexistent));
    }

    [Test]
    public void Scan_WhenCancellationTokenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        _tempDir.CreateFile("file1.pdf", "data");
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancelled token

        // Act & Assert
        Assert.Throws<OperationCanceledException>(() => _scanner.Scan(_tempDir.DirectoryPath, cts.Token));
    }
}

