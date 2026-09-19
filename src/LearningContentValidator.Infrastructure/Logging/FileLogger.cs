using Microsoft.Extensions.Logging;

namespace LearningContentValidator.Infrastructure.Logging;

/// <summary>
/// Lightweight, thread-safe file logger implementing Microsoft.Extensions.Logging.ILogger.
/// Appends structured log entries with timestamps, severity, and exception details to a specified log file.
/// </summary>
public sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public FileLogger(string categoryName, string logFilePath)
    {
        _categoryName = categoryName;
        _logFilePath = logFilePath;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string message = formatter(state, exception);
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logEntry = $"[{timestamp}] [{logLevel,-11}] [{_categoryName}] {message}";

        if (exception != null)
        {
            logEntry += Environment.NewLine + exception.ToString();
        }

        lock (_lock)
        {
            try
            {
                string? dir = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Defensive: logging failure must never bring down application execution
            }
        }
    }
}

