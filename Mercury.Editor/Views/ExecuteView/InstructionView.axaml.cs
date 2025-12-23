using Avalonia.Controls;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.ExecuteView;

public partial class InstructionView : UserControl {
    public InstructionView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<InstructionViewModel>();
    }
    
    public InstructionViewModel ViewModel { get; set; }

    private void DataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        InstructionDataGrid.ScrollIntoView(InstructionDataGrid.SelectedItem, null);
    }
    private void InstructionDataGrid_OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e) {
        e.PointerPressedEventArgs.Handled = true;
    }
}