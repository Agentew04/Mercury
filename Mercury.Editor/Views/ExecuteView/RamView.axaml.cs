using Avalonia.Controls;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.ExecuteView;

public partial class RamView : BaseControl<RamView, RamViewModel> {
    public RamView() {
        InitializeComponent();
    }
    
    private void DataGrid_OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e) {
        e.PointerPressedEventArgs.Handled = true;
    }
}