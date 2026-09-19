using System.Windows;
using LearningContentValidator.App.ViewModels;

namespace LearningContentValidator.App.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// Strictly follows MVVM best practices: contains NO business, validation, or file-system logic.
/// View state and behavior are entirely driven by MainViewModel through WPF data bindings.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

