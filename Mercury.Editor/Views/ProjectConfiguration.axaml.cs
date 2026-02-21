using Avalonia.Controls;
using Avalonia.Interactivity;
using Mercury.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views;

public partial class ProjectConfiguration : BaseWindow<ProjectConfiguration, ProjectConfigurationViewModel> {
    public ProjectConfiguration() {
        InitializeComponent();
    }

    private void OnWindowLoad(object? sender, RoutedEventArgs e) {
        ViewModel.Load();
    }
}