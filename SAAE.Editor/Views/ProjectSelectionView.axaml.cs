using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class ProjectSelectionView : Window {
    public ProjectSelectionView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<ProjectSelectionViewModel>();
        ViewModel.SetView(this);
        TitleBar.Window = this;
    }

    public ProjectSelectionViewModel ViewModel { get; set; }

    private async void BrowseFolderOnNewProject(object? sender, RoutedEventArgs e) {
        // TODO: mover isso para viewmodel
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
        ViewModel.NewProjectPath = path;
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e) {
        ViewModel.OverrideTaskCompletion();
    }
}