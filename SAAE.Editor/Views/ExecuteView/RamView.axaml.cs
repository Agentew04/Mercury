using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels.Execute;

namespace SAAE.Editor.Views.ExecuteView;

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