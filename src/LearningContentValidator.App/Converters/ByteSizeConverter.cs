using System.Globalization;
using System.Windows.Data;
using LearningContentValidator.Domain.Models;

namespace LearningContentValidator.App.Converters;

/// <summary>
/// Converts raw integer/long byte values into formatted file size strings (e.g. 1.25 MB).
/// </summary>
public sealed class ByteSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long bytes)
            return ContentFile.FormatByteSize(bytes);

        if (value is int intBytes)
            return ContentFile.FormatByteSize(intBytes);

        return "0 B";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

