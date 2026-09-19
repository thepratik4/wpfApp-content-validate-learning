using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace LearningContentValidator.App.Services;

/// <summary>
/// Concrete WPF implementation of IDialogService using Windows native dialogs.
/// </summary>
public sealed class WindowsDialogService : IDialogService
{
    public string? SelectFolder(string? initialDirectory = null)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Learning Content Package Directory",
            Multiselect = false
        };

        if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
        {
            dialog.InitialDirectory = initialDirectory;
        }

        bool? result = dialog.ShowDialog();
        return result == true ? dialog.FolderName : null;
    }

    public string? SelectSaveFile(string title, string defaultExtension, string filter)
    {
        var dialog = new SaveFileDialog
        {
            Title = title,
            DefaultExt = defaultExtension,
            Filter = filter,
            AddExtension = true,
            OverwritePrompt = true
        };

        bool? result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }

    public void ShowInformation(string message, string title = "Information")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "Error")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string title = "Warning")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
