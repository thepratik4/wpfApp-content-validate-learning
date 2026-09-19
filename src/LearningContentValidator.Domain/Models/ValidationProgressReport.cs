namespace LearningContentValidator.Domain.Models;

/// <summary>
/// Progress payload communicated back to the UI during an active validation run.
/// Demonstrates clean event/progress reporting decoupled from UI controls.
/// </summary>
public sealed class ValidationProgressReport
{
    public int Current { get; }
    public int Total { get; }
    public string CurrentFileName { get; }
    public double Percentage => Total > 0 ? ((double)Current / Total) * 100.0 : 0.0;

    public ValidationProgressReport(int current, int total, string currentFileName)
    {
        Current = current;
        Total = total;
        CurrentFileName = currentFileName ?? string.Empty;
    }
}

