using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LearningContentValidator.App.Services;
using LearningContentValidator.App.ViewModels;
using LearningContentValidator.App.Views;
using LearningContentValidator.Domain.Interfaces;
using LearningContentValidator.Domain.Models;
using LearningContentValidator.Domain.Validation.Rules;
using LearningContentValidator.Infrastructure.FileSystem;
using LearningContentValidator.Infrastructure.Logging;
using LearningContentValidator.Infrastructure.Reporting;
using LearningContentValidator.Infrastructure.Validation;

namespace LearningContentValidator.App;

/// <summary>
/// Application entry point and Composition Root.
/// Configures Microsoft.Extensions.DependencyInjection and logging,
/// registers domain, infrastructure, and presentation services,
/// and presents the main application window.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private ILogger<App>? _logger;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global unhandled exception handler for UI thread
        DispatcherUnhandledException += App_DispatcherUnhandledException;

        var services = new ServiceCollection();
        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();

        _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        _logger.LogInformation("Learning Content Validator application started.");

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // 1. Configure Structured Logging (Console + File)
        string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "validator.log");
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddConsole();
            builder.AddFile(logPath);
        });

        // 2. Configure Domain Options
        services.AddSingleton(Options.Create(new ValidationOptions()));

        // 3. Register File Scanning Infrastructure
        services.AddSingleton<IFileScanner, FileScanner>();

        // 4. Register Extensible Validation Rules
        // Notice how new rules can be added here without modifying ContentValidationService!
        services.AddSingleton<IValidationRule, FileSizeRule>();
        services.AddSingleton<IValidationRule, ExtensionRule>();
        services.AddSingleton<IValidationRule, EmptyFileRule>();
        services.AddSingleton<IValidationRule, FileNameRule>();
        services.AddSingleton<IValidationRule, RequiredMetadataRule>();
        services.AddSingleton<IValidationRule, DuplicateFileNameRule>();

        // 5. Register Content Validation Orchestration Engine
        services.AddSingleton<IContentValidationService, ContentValidationService>();

        // 6. Register Reporting Strategies and Factory
        services.AddSingleton<IReportGenerator, JsonReportGenerator>();
        services.AddSingleton<IReportGenerator, CsvReportGenerator>();
        services.AddSingleton<ReportGeneratorFactory>();

        // 7. Register UI Services
        services.AddSingleton<IDialogService, WindowsDialogService>();

        // 8. Register ViewModels and Views
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger?.LogCritical(e.Exception, "Unhandled application exception caught at UI boundary.");
        MessageBox.Show(
            $"An unexpected error occurred:\n{e.Exception.Message}\n\nTechnical details have been logged.",
            "Fatal Application Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.LogInformation("Learning Content Validator application shutting down.");
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

