using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.App.ViewModels;

/// <summary>
/// ViewModel wrapping a single ValidationResult for display within the WPF Results DataGrid.
/// Separates the raw domain model from presentation-specific properties and formatting.
/// </summary>
public sealed class FileResultViewModel : ViewModelBase
{
    public ValidationResult Result { get; }

    public string FileName => Result.FileName;
    public string FilePath => Result.FilePath;
    public ContentFileType FileType => Result.File.FileType;
    public string FormattedSize => Result.File.FormattedSize;
    public long FileSizeBytes => Result.File.FileSize;
    public string StatusText => Result.StatusText;
    public bool IsValid => Result.IsValid;
    public int ErrorCount => Result.ErrorCount;
    public int WarningCount => Result.WarningCount;
    public IReadOnlyList<string> Errors => Result.Errors;
    public IReadOnlyList<string> Warnings => Result.Warnings;
    public IReadOnlyList<ValidationIssue> Issues => Result.Issues;

    public FileResultViewModel(ValidationResult result)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }
}

