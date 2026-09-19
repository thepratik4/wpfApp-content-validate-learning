using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using LearningContentValidator.App.Services;
using LearningContentValidator.Domain.Interfaces;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Infrastructure.Reporting;

namespace LearningContentValidator.App.ViewModels;

/// <summary>
/// Primary ViewModel for the Learning Content Validator main window.
/// Demonstrates:
/// - Dependency Injection (Constructor Injection)
/// - Asynchronous UI operations without thread blocking (async/await)
/// - Cooperative task cancellation (CancellationTokenSource)
/// - Observable collections and dynamic LINQ filtering
/// - Clean separation of concerns (no UI controls referenced)
/// </summary>
public sealed class MainViewModel : ViewModelBase
{
    private readonly IFileScanner _fileScanner;
    private readonly IContentValidationService _validationService;
    private readonly ReportGeneratorFactory _reportGeneratorFactory;
    private readonly IDialogService _dialogService;
    private readonly ILogger<MainViewModel> _logger;

    private string _selectedFolderPath = string.Empty;
    private bool _isBusy;
    private bool _isIndeterminate;
    private double _progressPercentage;
    private string _statusMessage = "Ready. Select a folder to begin validation.";
    private ValidationSummary _summary = ValidationSummary.Empty;
    private FileResultViewModel? _selectedResult;
    private string _selectedFilter = "All";
    private CancellationTokenSource? _cancellationTokenSource;

    private readonly List<FileResultViewModel> _allResults = new();
    public ObservableCollection<FileResultViewModel> FilteredResults { get; } = new();

    public MainViewModel(
        IFileScanner fileScanner,
        IContentValidationService validationService,
        ReportGeneratorFactory reportGeneratorFactory,
        IDialogService dialogService,
        ILogger<MainViewModel> logger)
    {
        _fileScanner = fileScanner ?? throw new ArgumentNullException(nameof(fileScanner));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _reportGeneratorFactory = reportGeneratorFactory ?? throw new ArgumentNullException(nameof(reportGeneratorFactory));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize commands
        BrowseFolderCommand = new RelayCommand(BrowseFolder, () => !IsBusy);
        StartValidationCommand = new AsyncRelayCommand(StartValidationAsync, () => CanStartValidation, OnValidationException);
        CancelValidationCommand = new RelayCommand(CancelValidation, () => IsBusy);
        ExportJsonReportCommand = new AsyncRelayCommand(() => ExportReportAsync("JSON"), () => HasResults && !IsBusy);
        ExportCsvReportCommand = new AsyncRelayCommand(() => ExportReportAsync("CSV"), () => HasResults && !IsBusy);
        SetFilterCommand = new RelayCommand(param => SetFilter(param?.ToString() ?? "All"));
    }

    #region Observable Properties

    public string SelectedFolderPath
    {
        get => _selectedFolderPath;
        set
        {
            if (SetProperty(ref _selectedFolderPath, value))
            {
                OnPropertyChanged(nameof(CanStartValidation));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanStartValidation));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        private set => SetProperty(ref _isIndeterminate, value);
    }

    public double ProgressPercentage
    {
        get => _progressPercentage;
        private set => SetProperty(ref _progressPercentage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ValidationSummary Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    public FileResultViewModel? SelectedResult
    {
        get => _selectedResult;
        set => SetProperty(ref _selectedResult, value);
    }

    public string SelectedFilter
    {
        get => _selectedFilter;
        private set
        {
            if (SetProperty(ref _selectedFilter, value))
            {
                ApplyFilter();
            }
        }
    }

    public bool CanStartValidation => !IsBusy && !string.IsNullOrWhiteSpace(SelectedFolderPath);
    public bool HasResults => _allResults.Count > 0;

    #endregion

    #region Commands

    public ICommand BrowseFolderCommand { get; }
    public ICommand StartValidationCommand { get; }
    public ICommand CancelValidationCommand { get; }
    public ICommand ExportJsonReportCommand { get; }
    public ICommand ExportCsvReportCommand { get; }
    public ICommand SetFilterCommand { get; }

    #endregion

    #region Command Execution Methods

    private void BrowseFolder()
    {
        string? folder = _dialogService.SelectFolder(SelectedFolderPath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            SelectedFolderPath = folder;
            StatusMessage = $"Selected folder: {folder}";
            _logger.LogInformation("User selected folder: {Folder}", folder);
        }
    }

    private async Task StartValidationAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolderPath) || !Directory.Exists(SelectedFolderPath))
        {
            _dialogService.ShowError("The selected folder does not exist. Please choose a valid directory.", "Directory Not Found");
            return;
        }

        _logger.LogInformation("Validation workflow initiated for folder: {Folder}", SelectedFolderPath);

        IsBusy = true;
        IsIndeterminate = true;
        ProgressPercentage = 0;
        StatusMessage = "Scanning directory structure for content files...";
        _allResults.Clear();
        FilteredResults.Clear();
        SelectedResult = null;
        Summary = ValidationSummary.Empty;

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            // Step 1: Scan directory recursively on background thread
            var scanProgress = new Progress<int>(count =>
            {
                StatusMessage = $"Scanning directory... Found {count} files so far.";
            });

            var files = await _fileScanner.ScanAsync(SelectedFolderPath, token, scanProgress);

            if (files.Count == 0)
            {
                StatusMessage = "No content files discovered in the selected directory.";
                _logger.LogInformation("No files found during scan of: {Folder}", SelectedFolderPath);
                return;
            }

            _logger.LogInformation("File scan completed. Found {Count} files. Starting rule validation...", files.Count);

            // Step 2: Validate discovered files against validation rules
            IsIndeterminate = false;
            var validationProgress = new Progress<ValidationProgressReport>(report =>
            {
                ProgressPercentage = report.Percentage;
                StatusMessage = $"Validating ({report.Current}/{report.Total}): {report.CurrentFileName}";
            });

            var sessionResult = await _validationService.ValidateFilesAsync(
                files,
                overrideOptions: null,
                progress: validationProgress,
                cancellationToken: token);

            // Step 3: Populate Results and Summary
            Summary = sessionResult.Summary;

            _allResults.AddRange(sessionResult.Results.Select(r => new FileResultViewModel(r)));
            ApplyFilter();

            StatusMessage = $"Validation completed in {Summary.FormattedDuration}. " +
                            $"Total: {Summary.TotalFiles}, Valid: {Summary.ValidFiles}, " +
                            $"Invalid: {Summary.InvalidFiles}, Warnings: {Summary.WarningCount}.";

            OnPropertyChanged(nameof(HasResults));
            _logger.LogInformation("Validation session finished successfully.");
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Validation operation was cancelled by user.";
            _logger.LogWarning("Validation session was cancelled by the user.");
            _dialogService.ShowInformation("Validation was cancelled.", "Validation Cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error encountered during validation session.");
            StatusMessage = $"Validation failed: {ex.Message}";
            _dialogService.ShowError($"An error occurred during validation:\n{ex.Message}", "Validation Error");
        }
        finally
        {
            IsBusy = false;
            IsIndeterminate = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void CancelValidation()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            StatusMessage = "Cancelling validation operation...";
            _logger.LogInformation("User requested cancellation of current validation task.");
            _cancellationTokenSource.Cancel();
        }
    }

    private async Task ExportReportAsync(string format)
    {
        if (!HasResults)
        {
            _dialogService.ShowWarning("No validation results available to export. Run validation first.", "Export Report");
            return;
        }

        try
        {
            var generator = _reportGeneratorFactory.GetByFormat(format);
            string filter = format.Equals("JSON", StringComparison.OrdinalIgnoreCase)
                ? "JSON Files (*.json)|*.json|All Files (*.*)|*.*"
                : "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";

            string? targetPath = _dialogService.SelectSaveFile(
                title: $"Export Validation Report ({format})",
                defaultExtension: generator.DefaultExtension,
                filter: filter);

            if (string.IsNullOrWhiteSpace(targetPath))
                return;

            var report = new ValidationReport(
                scannedFolder: SelectedFolderPath,
                summary: Summary,
                results: _allResults.Select(vm => vm.Result).ToList(),
                optionsUsed: new ValidationOptions()
            );

            await generator.GenerateReportAsync(report, targetPath);

            _logger.LogInformation("Report successfully exported to: {Path}", targetPath);
            _dialogService.ShowInformation($"Report successfully exported to:\n{targetPath}", "Export Successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export report in format {Format}", format);
            _dialogService.ShowError($"Failed to export report:\n{ex.Message}", "Export Failed");
        }
    }

    private void SetFilter(string filter)
    {
        SelectedFilter = filter;
    }

    /// <summary>
    /// Demonstrates LINQ to Objects (Where, OrderBy) to filter and sort the presentation collection.
    /// In LINQ to Objects, query operators execute in-memory over IEnumerable&lt;T&gt; using delegate predicates.
    /// Unlike LINQ to Entities (database), there is no SQL translation or Expression Tree parsing overhead.
    /// </summary>
    private void ApplyFilter()
    {
        FilteredResults.Clear();

        IEnumerable<FileResultViewModel> query = _allResults;

        query = SelectedFilter switch
        {
            "Errors Only" => query.Where(r => !r.IsValid),
            "Warnings Only" => query.Where(r => r.IsValid && r.WarningCount > 0),
            "Valid Only" => query.Where(r => r.IsValid && r.WarningCount == 0),
            _ => query // "All"
        };

        // Sort invalid files first, then warning files, then valid files
        var sorted = query
            .OrderByDescending(r => r.ErrorCount)
            .ThenByDescending(r => r.WarningCount)
            .ThenBy(r => r.FileName);

        foreach (var item in sorted)
        {
            FilteredResults.Add(item);
        }
    }

    private void OnValidationException(Exception ex)
    {
        _logger.LogError(ex, "Unexpected command execution fault in MainViewModel.");
        _dialogService.ShowError($"Unexpected command failure: {ex.Message}", "Error");
    }

    #endregion
}

