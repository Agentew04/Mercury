using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SAAE.Editor.Controls;

public partial class TitleBar : UserControl {

    public Window window;
    
    public TitleBar() {
        InitializeComponent();
        DataContext = this;
    }
    
    private void BeginDrag(object? sender, PointerPressedEventArgs e) {
        window.BeginMoveDrag(e);
    }

    private void Minimize(object? sender, RoutedEventArgs e) {
        if (Design.IsDesignMode) {
            return;
        }

        window.WindowState = WindowState.Minimized;
    }

    private void Maximize(object? sender, RoutedEventArgs e) {
        if (Design.IsDesignMode) {
            return;
        }

        if (window.CanResize) {
            window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }

    private void Close(object? sender, RoutedEventArgs e) {
        if (Design.IsDesignMode) {
            return;
        }
        
        window.Close();
    }
}