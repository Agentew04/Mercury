using System;
using System.Globalization;
using System.Numerics;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace SAAE.Editor.Converters;

public class HexadecimalConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is int s32) {
            return s32.ToString("X8");
        }else if (value is uint u32) {
            return u32.ToString("X8");
        }else if (value is long s64) {
            return s64.ToString("X16");
        }else if (value is long u64) {
            return u64.ToString("X16");
        }else if (value is byte s8) {
            return s8.ToString("X2");
        }

        return BindingNotification.Null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return BindingNotification.Null;
        // if (value is not string s) {
        //     return 0;
        // }
        //
        // byte[] bytes = System.Convert.FromHexString(s);
        // if (bytes.Length == 4) {
        //     return BitConverter.ToInt32(bytes);
        // }else if (bytes.Length == 8) {
        //     return BitConverter.ToInt64(bytes);
        // }else if (bytes.Length == 1) {
        //     return bytes[0];
        // }
        //
        // Console.WriteLine("FUDEUEUUUUUUUUUUUUUUUUUUUUUUUUU!!!!!!!!!!!!!!!!!!!!!!");
        // return -1;
    }
}