using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Mercury.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.Views;

public partial class ProjectSelectionView : Window {
    public ProjectSelectionView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<ProjectSelectionViewModel>();
        ViewModel.SetView(this);
        TitleBar.Window = this;
    }

    private readonly ILogger<ProjectSelectionView> logger = App.Services.GetRequiredService<ILogger<ProjectSelectionView>>();
    public ProjectSelectionViewModel ViewModel { get; set; }

    private async void BrowseFolderOnNewProject(object? sender, RoutedEventArgs e) {
        if(!StorageProvider.CanOpen){
            logger.LogError("FilePicker is not supported on this platform");
            return;
        }
        
        IReadOnlyList<IStorageFolder> result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() {
            Title = Localization.ProjectResources.SelectFolderPickerValue,
            AllowMultiple = false,
        });

        if (result.Count != 1) {
            return;
        }

        string path = result[0].Path.AbsolutePath;
        ViewModel.NewProjectPath = path;
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e) {
        ViewModel.OverrideTaskCompletion();
    }
}