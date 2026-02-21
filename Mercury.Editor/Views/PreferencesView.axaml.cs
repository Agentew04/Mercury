using Avalonia.Controls;
using Avalonia.Interactivity;
using Mercury.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views;

public partial class PreferencesView : BaseWindow<PreferencesView, PreferencesViewModel> {
    public PreferencesView() {
        InitializeComponent();
    }

    private void OnWindowLoad(object? sender, RoutedEventArgs e) {
        ViewModel.Load();
    }
}