using System;
using System.Globalization;
using Avalonia;
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

public class ColorConverterParam : AvaloniaObject {
    public static readonly StyledProperty<IBrush?> TrueBrushProperty =
        AvaloniaProperty.Register<ColorConverterParam, IBrush?>(nameof(TrueBrush));

    public static readonly StyledProperty<IBrush?> FalseBrushProperty =
        AvaloniaProperty.Register<ColorConverterParam, IBrush?>(nameof(FalseBrush));

    public IBrush? TrueBrush
    {
        get => GetValue(TrueBrushProperty);
        set => SetValue(TrueBrushProperty, value);
    }

    public IBrush? FalseBrush
    {
        get => GetValue(FalseBrushProperty);
        set => SetValue(FalseBrushProperty, value);
    }
}