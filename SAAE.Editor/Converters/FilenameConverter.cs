using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace SAAE.Editor.Converters;

/// <summary>
/// Converter that gets the filename from an absolute path.
/// </summary>
public class FilenameConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value is not string fullpath ? null : Path.GetFileName(fullpath);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}