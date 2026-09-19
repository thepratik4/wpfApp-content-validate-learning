using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace LearningContentValidator.Infrastructure.Logging;

/// <summary>
/// Provider managing FileLogger instances per category.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public FileLoggerProvider(string logFilePath)
    {
        _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _logFilePath));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

