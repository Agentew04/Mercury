using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Mercury.Editor.Converters;

public class BoolToBoldConverter : IValueConverter{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not bool b) {
            return FontWeight.Regular;
        }
        return b ? FontWeight.Bold : FontWeight.Regular;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not FontWeight fw) {
            return false;
        }

        return fw switch {
            FontWeight.Regular => false,
            FontWeight.Bold => true,
            _ => false
        };
    }
}