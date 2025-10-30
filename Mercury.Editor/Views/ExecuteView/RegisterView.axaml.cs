using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.ExecuteView;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
        DataContext = viewModel = App.Services.GetRequiredService<RegisterViewModel>();
    }

    private RegisterViewModel viewModel;
    
    private void RowBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        Control? control = (Control?)sender;
        Register? reg = (Register?)control?.DataContext;
        if (reg is null || control is null) {
            return;
        }
        FlyoutBase? flyout = FlyoutBase.GetAttachedFlyout(control);
        FlyoutBase.ShowAttachedFlyout(control);
    }
}

