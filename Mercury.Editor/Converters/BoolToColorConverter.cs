using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Mercury.Editor.Converters;

public class BoolToColorConverter : IValueConverter{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not bool input || parameter is not ColorConverterParam param) {
            return BindingNotification.Null;
        }

        if (param.TrueBrush is null || param.FalseBrush is null) {
            return BindingNotification.Null;
        }
        return input ? param.TrueBrush : param.FalseBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not IBrush brushInput || parameter is not ColorConverterParam param) {
            return BindingNotification.Null;
        }
        if (param.TrueBrush is null || param.FalseBrush is null) {
            return BindingNotification.Null;
        }

        return ReferenceEquals(brushInput, param.TrueBrush);
    }
}

public class ColorConverterParam {
    public IBrush? TrueBrush { get; set; }
    public IBrush? FalseBrush { get; set; }
}