using Avalonia;
using Avalonia.Controls;
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

    private void DataGrid_OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e) {
        e.PointerPressedEventArgs.Handled = true;
    }
}