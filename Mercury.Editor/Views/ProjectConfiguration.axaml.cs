using Avalonia.Controls;
using Avalonia.Interactivity;
using Mercury.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views;

public partial class ProjectConfiguration : Window {
    public ProjectConfiguration() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<ProjectConfigurationViewModel>();
        ViewModel.SetView(this);
    }
    
    public ProjectConfigurationViewModel ViewModel { get; set; }

    private void OnWindowLoad(object? sender, RoutedEventArgs e) {
        ViewModel.Load();
    }
}