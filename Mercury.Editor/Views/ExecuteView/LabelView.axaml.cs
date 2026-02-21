using Avalonia.Controls;
using Mercury.Editor.Models.Messages;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.ExecuteView;

public partial class LabelView : BaseControl<LabelView, LabelViewModel> {
    public LabelView() {
        InitializeComponent();
    }
    
    private void DataGrid_OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e) {
        //e.PointerPressedEventArgs.Handled = true;
    }

    private void DataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (e.AddedItems.Count > 0)
        {
            Symbol? symbol = (Symbol?)e.AddedItems[0];
            if (symbol is not null) {
                ViewModel.OnSymbolClicked(symbol.Value);
            }
        }
        LabelsDataGrid.SelectedIndex = -1;
    }
}