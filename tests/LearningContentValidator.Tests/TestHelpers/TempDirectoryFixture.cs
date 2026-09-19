namespace LearningContentValidator.Tests.TestHelpers;

/// <summary>
/// Helper fixture that creates an isolated temporary directory for file-system testing.
/// Implements IDisposable to ensure deterministic teardown and deletion after tests run.
/// Analogous to RAII (Resource Acquisition Is Initialization) pattern in C++.
/// </summary>
public sealed class TempDirectoryFixture : IDisposable
{
    public string DirectoryPath { get; }

    public TempDirectoryFixture()
    {
        DirectoryPath = Path.Combine(Path.GetTempPath(), "LcvTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DirectoryPath);
    }

    public string CreateFile(string relativePath, string content = "")
    {
        string fullPath = Path.Combine(DirectoryPath, relativePath);
        string? parent = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
        {
            Directory.CreateDirectory(parent);
        }

        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    public string CreateFile(string relativePath, byte[] bytes)
    {
        string fullPath = Path.Combine(DirectoryPath, relativePath);
        string? parent = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
        {
            Directory.CreateDirectory(parent);
        }

        File.WriteAllBytes(fullPath, bytes);
        return fullPath;
    }

    public string CreateDirectory(string relativePath)
    {
        string fullPath = Path.Combine(DirectoryPath, relativePath);
        Directory.CreateDirectory(fullPath);
        return fullPath;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
        catch
        {
            // Best effort cleanup in case of file locks in CI
        }
    }
}

