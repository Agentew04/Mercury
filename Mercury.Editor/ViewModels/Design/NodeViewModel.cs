using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Views.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.ViewModels.Design;

public partial class NodeViewModel : ObservableObject {
    
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private bool isEditingName;
    [ObservableProperty] private Point location;
    // used to make the editwindow a dialog to the element in the mainwindow.
    public TopLevel? TopLevel { get; set; }
    public ObservableCollection<ConnectorViewModel> Input { get; set; } = [];
    public ObservableCollection<ConnectorViewModel> Output { get; set; } = [];

    [RelayCommand]
    private void Edit() {
        EditNodeView editNodeView = App.Services.GetRequiredService<EditNodeView>();
        // editNodeView.ViewModel.
        if (TopLevel is Window w) {
            editNodeView.ShowDialog(w);
        }
        else {
            editNodeView.Show();
        }
    }
}