using Avalonia.Controls;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.ExecuteView;

public partial class ExecuteView : UserControl {
    public ExecuteView() {
        InitializeComponent();
        DataContext = vm = App.Services.GetRequiredService<ExecuteViewModel>();
        vm.LoadSizes();
        Unloaded += (_, _) => vm.OnUnload();
    }
    
    private ExecuteViewModel vm;
}