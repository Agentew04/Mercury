using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

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