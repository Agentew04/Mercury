using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Converters;

public class ProjectNodeTypeToIcon : IValueConverter {
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not NodeType nodeType) {
            return null;
        }

        return nodeType switch {
            NodeType.None => "",
            NodeType.Category => ((char)0xE2CE).ToString(), // 'info' icon
            NodeType.Folder => ((char)0xE24A).ToString(), // 'folder' icon
            NodeType.AssemblyFile => ((char)0xE914).ToString(), // 'file-code' icon
            NodeType.UnknownFile => ((char)0xE230).ToString(), // 'file' icon
            _ => "ERRO"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new InvalidOperationException();
    }
}