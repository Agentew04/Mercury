using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using SAAE.Editor.Views;

namespace SAAE.Editor.ViewModels;

public partial class ProjectFileVisualItem : ObservableObject {

    public ProjectFileVisualItem(ProjectFile project, ICommand openCommand) {
        ProjectFile = project;
        OpenCommand = openCommand;
    }
    
    [ObservableProperty] private ProjectFile projectFile;
    [ObservableProperty] private ICommand openCommand;
}

public partial class ProjectSelectionViewModel : BaseViewModel {

    public ProjectSelectionView view = null!; // isso eh feio mas nao quero fazer um role pro filepicker
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    
    [ObservableProperty]
    private string searchQuery = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyRecentProjects))]
    private ObservableCollection<ProjectFileVisualItem> filteredRecentProjects = [];
    private readonly List<ProjectFileVisualItem> allRecentProjects = [];
    
    [ObservableProperty]
    private bool isCreatingProject;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DirectoryNotice))]
    private string newProjectName = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DirectoryNotice))]
    private string newProjectPath = string.Empty;

    [ObservableProperty] 
    private ObservableCollection<string> operatingSystems = [];
    [ObservableProperty]
    private int selectedOperatingSystemIndex = -1;
    
    public bool EmptyRecentProjects => FilteredRecentProjects.Count == 0;

    private readonly TaskCompletionSource<bool> projectSelectionTask = new();
    
    public string DirectoryNotice {
        get {
            string path = Path.Combine(SanitizeProjectPath(NewProjectPath), SanitizeProjectName(NewProjectName));
            return string.Format(Localization.ProjectResources.DirectoryResultNoticeValue, path);
        }
    }

    public ProjectSelectionViewModel() {
        foreach (ProjectFile project in projectService.GetRecentProjects()) {
            filteredRecentProjects.Add(new ProjectFileVisualItem(project, OpenProjectCommand));
            allRecentProjects.Add(new ProjectFileVisualItem(project, OpenProjectCommand));
        }
    }
    
    partial void OnSearchQueryChanged(string value) {
        // atualiza a lista de projetos recentes
        filteredRecentProjects.Clear();
        foreach (ProjectFileVisualItem proj in allRecentProjects) {
            bool nameCheck = proj.ProjectFile.ProjectName.Contains(value, StringComparison.OrdinalIgnoreCase);
            bool pathCheck = proj.ProjectFile.ProjectPath.Contains(value, StringComparison.OrdinalIgnoreCase);
            
            if (nameCheck || pathCheck) {
                filteredRecentProjects.Add(proj);
            }
        }
    }

    public Task WaitForProjectSelection() {
        return projectSelectionTask.Task;
    }
    
    [RelayCommand]
    private void NewProjectStart() {
        IsCreatingProject = true;
        OperatingSystems = ["Mars 4.5", "Linux Kernel 1.0"];
    }

    [RelayCommand]
    private void NewProjectEnd() {
        string path = SanitizeProjectPath(NewProjectPath);
        string name = SanitizeProjectName(NewProjectName);
        string projectFilePath = Path.Combine(path, name);
        string os = OperatingSystems[SelectedOperatingSystemIndex];
        var project = projectService.CreateProject(path, name, os);
        IsCreatingProject = false;
    }
    
    [RelayCommand]
    private async Task OpenProjectDialog() {
        if (!view.StorageProvider.CanOpen) {
            Console.WriteLine("FilePicker nao eh suportado nessa plataforma!");
            return;
        }
        
        IReadOnlyList<IStorageFile> result = await view.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = "Open Project File",
            AllowMultiple = false,
            SuggestedFileName = "project.asmproj", 
            FileTypeFilter = [
                new FilePickerFileType("Assembly Projects") {
                    Patterns = [ "*.asmproj" ], 
                }
            ]
        });

        if (result.Count != 1) {
            return;
        }

        string path = result[0].Path.AbsolutePath;
        OpenProject(path);
    }
    
    [RelayCommand]
    private void OpenProject(string path) {
        if (!path.EndsWith(".asmproj")) {
            // esse check nao precisaria, mas melhor garantir
            return;
        }

        ProjectFile? project = projectService.OpenProject(path);
        if (project is null) {
            // msg de erro ao usuario
            return;
        }
        projectService.SetCurrentProject(project);
        if(!projectSelectionTask.Task.IsCompleted) {
            projectSelectionTask.SetResult(true);
        }
    }

    private static string SanitizeProjectName(string name) {
        return Path.GetInvalidFileNameChars().Aggregate(name, (current, illegal) => current.Replace(illegal.ToString(), ""));
    }

    private static string SanitizeProjectPath(string path) {
        return Path.GetInvalidPathChars().Aggregate(path, (current, illegal) => current.Replace(illegal.ToString(), ""));
    }
}