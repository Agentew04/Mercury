using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class ProjectSelectionView : Window {
    public ProjectSelectionView() {
        InitializeComponent();
        DataContext = SelectionViewModel = App.Services.GetRequiredService<ProjectSelectionViewModel>();
        SelectionViewModel.view = this;
    }

    public ProjectSelectionViewModel SelectionViewModel { get; set; }

    private async void BrowseFolderOnNewProject(object? sender, RoutedEventArgs e) {
        if(!StorageProvider.CanOpen){
            Console.WriteLine("FilePicker nao eh suportado nessa plataforma!");
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
        SelectionViewModel.NewProjectPath = path;
    }
}