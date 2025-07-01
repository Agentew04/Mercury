using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SAAE.Engine;

namespace SAAE.Editor.Converters;

public class ArchitectureToString : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not Architecture architecture) {
            return "WTF";
        }

        return architecture switch {
            Architecture.Mips => "MIPS",
            Architecture.RiscV => "RISC-V",
            Architecture.Arm => "ARM",
            Architecture.Unknown => "UNKNOWN",
            _ => "ERRO"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not string architectureString) {
            return Architecture.Unknown;
        }

        return architectureString switch {
            "MIPS" => Architecture.Mips,
            "RISC-V" => Architecture.RiscV,
            "ARM" => Architecture.Arm,
            _ => Architecture.Unknown
        };
        
    }
}