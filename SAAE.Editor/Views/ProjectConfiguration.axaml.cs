using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class ProjectConfiguration : Window {
    public ProjectConfiguration() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<ProjectConfigurationViewModel>();
        ViewModel.View = this;
    }
    
    public ProjectConfigurationViewModel ViewModel { get; set; }

    private void OnWindowLoad(object? sender, RoutedEventArgs e) {
        ViewModel.Load();
    }
}