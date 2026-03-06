using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Editor.ViewModels.Design;

public partial class ConnectorViewModel : ObservableObject {
    public string Title { get; set; } = string.Empty;
    
    [ObservableProperty]
    private Point anchor;

    [ObservableProperty]
    private bool isConnected;
}