using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Mercury.Editor.Converters;

public class HexadecimalConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value switch {
            int s32 => s32.ToString("X8"),
            uint u32 => u32.ToString("X8"),
            long s64 => s64.ToString("X16"),
            ulong u64 => u64.ToString("X16"),
            byte s8 => s8.ToString("X2"),
            _ => BindingNotification.Null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return BindingNotification.Null;
    }
}