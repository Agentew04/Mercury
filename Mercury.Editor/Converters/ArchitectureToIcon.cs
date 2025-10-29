using System;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Mercury.Engine.Common;
using Mercury.Engine;

namespace Mercury.Editor.Converters;

public class ArchitectureToIconSource : IValueConverter{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not Architecture architecture) {
            return BindingNotification.Null;
        }

        string uri =  architecture switch {
            Architecture.Mips => "avares://Mercury.Editor/Assets/Images/mips-logo.png",
            Architecture.RiscV => "avares://Mercury.Editor/Assets/Images/riscv-logo.png",
            Architecture.Arm => "avares://Mercury.Editor/Assets/Images/arm-logo.png",
            _ => "avares://Mercury.Editor/Assets/Images/book.png"
        };
        var img = new Bitmap(AssetLoader.Open(new Uri(uri)));
        return img;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not string iconSource) {
            return BindingNotification.Null;
        }

        Architecture arch = iconSource switch {
            "avares://Mercury.Editor/Assets/Images/mips-logo.png" => Architecture.Mips,
            "avares://Mercury.Editor/Assets/Images/riscv-logo.png" => Architecture.RiscV,
            "avares://Mercury.Editor/Assets/Images/arm-logo.png" => Architecture.Arm,
            _ => Architecture.Mips
        };
        return arch;
    }
}
