using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels.Execute;

namespace SAAE.Editor.Views.ExecuteView;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
        DataContext = viewModel = App.Services.GetRequiredService<RegisterViewModel>();
    }

    private RegisterViewModel viewModel;

    // private void DataGrid_OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e) {
    //     e.PointerPressedEventArgs.Handled = true;
    // }

    private void DataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if(sender is DataGrid dataGrid && e.AddedItems.Count > 0){
            if (e.AddedItems[0] is Register register)
            {
                FlyoutBase? flyout = FlyoutBase.GetAttachedFlyout(dataGrid);
                FlyoutBase.ShowAttachedFlyout(dataGrid);
                dataGrid.SelectedIndex = -1;
            }
        }
    }
}