using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Editor.ViewModels.Design;

public partial class ConnectorViewModel : ObservableObject {

    [ObservableProperty] 
    private string title = string.Empty;

    [ObservableProperty] private int bitWidth;
    
    [ObservableProperty]
    private Point anchor;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty] 
    private ConnectorType type;
}

public enum ConnectorType {
    Input,
    Output,
}