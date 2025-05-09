using System;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SAAE.Engine;

namespace SAAE.Editor.Converters;

public class ArchitectureToIconSource : IValueConverter{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not Architecture architecture) {
            return null;
        }

        string uri =  architecture switch {
            Architecture.Mips => "avares://SAAE.Editor/Assets/Images/mips-logo.png",
            Architecture.RiscV => "avares://SAAE.Editor/Assets/Images/riscv-logo.png",
            Architecture.Arm => "avares://SAAE.Editor/Assets/Images/arm-logo.png",
            _ => "avares://SAAE.Editor/Assets/Images/book.png"
        };
        var img = new Bitmap(AssetLoader.Open(new Uri(uri)));
        return img;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not string iconSource) {
            return null;
        }

        var arch = iconSource switch {
            "avares://SAAE.Editor/Assets/Images/mips-logo.png" => Architecture.Mips,
            "avares://SAAE.Editor/Assets/Images/riscv-logo.png" => Architecture.RiscV,
            "avares://SAAE.Editor/Assets/Images/arm-logo.png" => Architecture.Arm,
            _ => Architecture.Mips
        };
        return arch;
    }
}

public class Converter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is null) {
            return "value is null";
        }
        
        if (value is not ICommand command) {
            return "N eh cmd";
        }

        return command.GetType().Name;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}