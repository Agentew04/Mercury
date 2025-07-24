using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.ViewModels.Execute;

namespace SAAE.Editor.Views.ExecuteView;

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