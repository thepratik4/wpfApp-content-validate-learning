using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LearningContentValidator.Infrastructure.Logging;

/// <summary>
/// Extension methods for registering FileLoggerProvider with the Microsoft.Extensions.Logging infrastructure.
/// </summary>
public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string filePath)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new FileLoggerProvider(filePath));
        return builder;
    }
}
