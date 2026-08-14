using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Localization;
using Mercury.Editor.Views.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.ViewModels.Design;

public partial class NodeViewModel : ObservableObject {
    [ObservableProperty] 
    private string name = string.Empty;
    
    [ObservableProperty] 
    private bool isEditingName;
    
    [ObservableProperty] 
    private Point location;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(NodeTypeLocalized))]
    private NodeType nodeType = NodeType.Combinational;
    
    [ObservableProperty] 
    private string code = string.Empty;

    public string NodeTypeLocalized => NodeType switch {
        NodeType.Combinational => EditNodeResources.NodeTypeCombinationalValue,
        NodeType.Sequential => EditNodeResources.NodeTypeSequentialValue,
        _ => "node type not localizable"
    };

    private readonly EditorViewModel editorViewModel;

    public TopLevel? TopLevel { get; set; }
    
    public ObservableCollection<ConnectorViewModel> Input { get; set; } = [];
    public ObservableCollection<ConnectorViewModel> Output { get; set; } = [];

    public bool Dirty { get; set; } = false;

    public NodeViewModel(EditorViewModel editorViewModel) {
        this.editorViewModel = editorViewModel;
        LocalizationManager.CultureChanged += Localize;
    }

    private void Localize(CultureInfo _) {
        OnPropertyChanged(nameof(NodeTypeLocalized));
    }

    [RelayCommand]
    private async Task Edit() {
        EditNodeView editNodeView = App.Services.GetRequiredService<EditNodeView>();
        editNodeView.ViewModel.Load(this);
        if (TopLevel is Window w) {
            await editNodeView.ShowDialog(w);
            if (editNodeView.ViewModel.Applied) {
                Dirty = true;
                editorViewModel.RefreshChanges();
            }
        }
        else {
            editNodeView.Show();
        }

        
    }
}