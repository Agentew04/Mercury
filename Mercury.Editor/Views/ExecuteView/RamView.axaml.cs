using Avalonia.Controls;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.ExecuteView;

public partial class RamView : UserControl {
    public RamView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<RamViewModel>();
    }
    
    public RamViewModel ViewModel { get; private set; }

    private void DataGrid_OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e) {
        e.PointerPressedEventArgs.Handled = true;
    }
}