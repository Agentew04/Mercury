using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SAAE.Editor.Models;

namespace SAAE.Editor.Converters;

public class IsGuideMenuConverter : IValueConverter{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value is GuideMenu;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}

public class IsGuideChapterConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value is GuideChapter;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}