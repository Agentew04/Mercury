using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.ViewModels.Execute;

namespace SAAE.Editor.Views.ExecuteView;

public partial class LabelView : UserControl {
    public LabelView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<LabelViewModel>();
    }
    
    public LabelViewModel ViewModel { get; private set; }

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