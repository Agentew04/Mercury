using Avalonia.Controls;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.ExecuteView;

public partial class InstructionView : BaseControl<InstructionView, InstructionViewModel> {
    public InstructionView() {
        InitializeComponent();
    }
    
    private void DataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        InstructionDataGrid.ScrollIntoView(InstructionDataGrid.SelectedItem, null);
    }
    private void InstructionDataGrid_OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e) {
        e.PointerPressedEventArgs.Handled = true;
    }
}