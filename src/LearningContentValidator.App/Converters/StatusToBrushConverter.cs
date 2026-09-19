using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LearningContentValidator.App.Converters;

/// <summary>
/// Converts status strings ("Valid", "Warning", "Invalid") to matching UI brush colors.
/// </summary>
public sealed class StatusToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush ValidBrush = new(Color.FromRgb(34, 139, 34));   // Forest Green
    private static readonly SolidColorBrush WarningBrush = new(Color.FromRgb(217, 119, 6));  // Dark Amber
    private static readonly SolidColorBrush ErrorBrush = new(Color.FromRgb(220, 38, 38));    // Crimson Red
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(107, 114, 128)); // Slate Grey

    static StatusToBrushConverter()
    {
        ValidBrush.Freeze();
        WarningBrush.Freeze();
        ErrorBrush.Freeze();
        DefaultBrush.Freeze();
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string status = value?.ToString() ?? string.Empty;

        return status switch
        {
            "Valid" => ValidBrush,
            "Warning" => WarningBrush,
            "Invalid" => ErrorBrush,
            _ => DefaultBrush
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

