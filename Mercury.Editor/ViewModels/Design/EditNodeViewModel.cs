using System.Collections.ObjectModel;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Views.Design;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.ViewModels.Design;

public partial class EditNodeViewModel : BaseViewModel<EditNodeViewModel, EditNodeView> {

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private bool isBarrier;
    [ObservableProperty] private ObservableCollection<ConnectorViewModel> inputs = [];
    [ObservableProperty] private ObservableCollection<ConnectorViewModel> outputs = [];
    [ObservableProperty] private TextDocument behaviourDocument = new();
    
    [RelayCommand]
    private void Apply() {
        
    }

    [RelayCommand]
    private void Cancel() {
        EditNodeView? view = GetView();
        if (view is null) {
            Logger.LogError("Could not find view for EditNodeViewModel. How will we close?");
            return;
        }
        view.Close();
    }
}