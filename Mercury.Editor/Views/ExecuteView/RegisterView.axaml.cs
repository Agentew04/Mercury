using System;
using System.Drawing;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;
using Mercury.Editor.Localization;
using Rectangle = Avalonia.Controls.Shapes.Rectangle;

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

