using LearningContentValidator.Domain.Interfaces;

namespace LearningContentValidator.Infrastructure.Reporting;

/// <summary>
/// Factory resolving the appropriate IReportGenerator based on file format name or file extension.
/// Demonstrates the Factory Method pattern and IEnumerable&lt;T&gt; injection.
/// </summary>
public sealed class ReportGeneratorFactory
{
    private readonly Dictionary<string, IReportGenerator> _generatorsByFormat;
    private readonly Dictionary<string, IReportGenerator> _generatorsByExtension;

    public ReportGeneratorFactory(IEnumerable<IReportGenerator> generators)
    {
        if (generators == null)
            throw new ArgumentNullException(nameof(generators));

        _generatorsByFormat = new Dictionary<string, IReportGenerator>(StringComparer.OrdinalIgnoreCase);
        _generatorsByExtension = new Dictionary<string, IReportGenerator>(StringComparer.OrdinalIgnoreCase);

        foreach (var generator in generators)
        {
            _generatorsByFormat[generator.SupportedFormat] = generator;
            _generatorsByExtension[generator.DefaultExtension] = generator;
        }
    }

    public IReportGenerator GetByFormat(string format)
    {
        if (_generatorsByFormat.TryGetValue(format, out var generator))
            return generator;

        throw new NotSupportedException($"No report generator registered for format: '{format}'.");
    }

    public IReportGenerator GetByExtension(string extension)
    {
        string normalized = extension.StartsWith('.') ? extension : "." + extension;
        if (_generatorsByExtension.TryGetValue(normalized, out var generator))
            return generator;

        throw new NotSupportedException($"No report generator registered for extension: '{extension}'.");
    }
}

