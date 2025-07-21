using System;
using System.Globalization;
using System.IO;
using Avalonia.Data;
using Avalonia.Data.Converters;
using SAAE.Editor.Extensions;

namespace SAAE.Editor.Converters;

/// <summary>
/// Converter that gets the filename from an absolute path.
/// </summary>
public class FilenameConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value switch {
            string strPath => Path.GetFileName(strPath),
            PathObject objPath => objPath.FullFileName,
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        //return BindingNotification.
        throw new NotSupportedException();
    }
}