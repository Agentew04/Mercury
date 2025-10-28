using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Mercury.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views;

public partial class PreferencesView : Window {
    public PreferencesView() {
        InitializeComponent();
        DataContext = viewModel = App.Services.GetRequiredService<PreferencesViewModel>();
        viewModel.SetView(this);
    }

    private PreferencesViewModel viewModel;

    private void OnWindowLoad(object? sender, RoutedEventArgs e) {
        viewModel.Load();
    }
}