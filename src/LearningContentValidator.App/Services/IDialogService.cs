namespace LearningContentValidator.App.Services;

/// <summary>
/// Abstraction for user dialog interactions (folder selection, file save dialogs, alert messages).
/// Injected into ViewModels so they can trigger dialogs without coupling to concrete WPF window classes,
/// preserving testability.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Displays a folder browser dialog to select a folder.
    /// </summary>
    string? SelectFolder(string? initialDirectory = null);

    /// <summary>
    /// Displays a save file dialog to pick an export destination path.
    /// </summary>
    string? SelectSaveFile(string title, string defaultExtension, string filter);

    /// <summary>
    /// Displays an informational message box.
    /// </summary>
    void ShowInformation(string message, string title = "Information");

    /// <summary>
    /// Displays an error message box.
    /// </summary>
    void ShowError(string message, string title = "Error");

    /// <summary>
    /// Displays a warning message box.
    /// </summary>
    void ShowWarning(string message, string title = "Warning");
}

