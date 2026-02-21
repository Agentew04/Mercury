using Avalonia.Controls;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.ExecuteView;

public partial class ExecuteView : BaseControl<ExecuteView, ExecuteViewModel> {
    public ExecuteView() {
        InitializeComponent();
        ViewModel.LoadSizes();
        Unloaded += (_, _) => ViewModel.OnUnload();
    }
}