using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Extensions;
using Mercury.Editor.Localization;
using Mercury.Editor.Models;
using Mercury.Editor.Services;
using Mercury.Editor.Views;
using Mercury.Engine.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mercury.Engine;

namespace Mercury.Editor.ViewModels;

public partial class ProjectFileVisualItem : ObservableObject {

    public ProjectFileVisualItem(ProjectFile project, IRelayCommand<PathObject> openCommand) {
        ProjectFile = project;
        OpenCommand = openCommand;
    }
    
    [ObservableProperty] private ProjectFile projectFile;
    [ObservableProperty] private IRelayCommand<PathObject> openCommand;
}

public partial class ProjectSelectionViewModel : BaseViewModel<ProjectSelectionViewModel, ProjectSelectionView> {
    
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();

    public bool Cancelled { get; private set; } = false;
    
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
    [NotifyPropertyChangedFor(nameof(CanCreateProject))]
    private string newProjectName = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DirectoryNotice))]
    [NotifyPropertyChangedFor(nameof(CanCreateProject))]
    private string newProjectPath = string.Empty;

    private List<OperatingSystemType> allOperatingSystems = null!;
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(CanChangeOperatingSystem))]
    private ObservableCollection<OperatingSystemType> operatingSystems = [];
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateProject))]
    private int selectedOperatingSystemIndex = -1;

    [ObservableProperty] private ObservableCollection<Architecture> isas = [];
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateProject))]
    [NotifyPropertyChangedFor(nameof(CanChangeOperatingSystem))]
    private int selectedIsaIndex = -1;

    public ObservableCollection<Template> Templates { get; set; } = [];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChangeOperatingSystem))]
    private int selectedTemplateIndex;
    private readonly Template blankTemplate = Template.Blank;
    
    public bool EmptyRecentProjects => FilteredRecentProjects.Count == 0;
    
    public bool CanCreateProject => !string.IsNullOrWhiteSpace(NewProjectName) 
                                    && !string.IsNullOrWhiteSpace(NewProjectPath)
                                    && (
                                        Templates[SelectedTemplateIndex].IsBlank && SelectedOperatingSystemIndex >= 0
                                        || !Templates[SelectedTemplateIndex].IsBlank
                                        )
                                    && SelectedIsaIndex >= 0
                                    && SelectedTemplateIndex != -1;

    public bool CanChangeOperatingSystem => (SelectedTemplateIndex == -1 || (Templates.Count > SelectedTemplateIndex && Templates[SelectedTemplateIndex].IsBlank)) && OperatingSystems.Count > 0;
    

    private readonly TaskCompletionSource<bool> projectSelectionTask = new();
    
    public string DirectoryNotice {
        get {
            string path = Path.Combine(SanitizeProjectPath(NewProjectPath), SanitizeProjectName(NewProjectName));
            return string.Format(ProjectResources.DirectoryResultNoticeValue, path);
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
            bool pathCheck = proj.ProjectFile.ProjectPath.Parts.Any(x => x.Contains(value, StringComparison.OrdinalIgnoreCase));
            
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
        allOperatingSystems = OperatingSystemManager.GetAvailableOperatingSystems().ToList();
        OperatingSystems = new ObservableCollection<OperatingSystemType>(allOperatingSystems);
        Isas = [Architecture.Mips, Architecture.RiscV, Architecture.Arm];
        SelectedIsaIndex = Isas.IndexOf(Architecture.Mips);
        OnPropertyChanged(nameof(CanChangeOperatingSystem));
    }

    [RelayCommand]
    private void NewProjectReturn()
    {
        IsCreatingProject = false;
    }
    
    partial void OnSelectedIsaIndexChanged(int value) {
        if (value < 0 || value >= Isas.Count) {
            Logger.LogInformation("Invalid ISA index: {idx}", value);
            return;
        }
        Architecture isa = Isas[value];
        IEnumerable<OperatingSystemType> oss = allOperatingSystems
            .Where(x => x.CompatibleArchitecture == isa);
        OperatingSystems.Clear();
        foreach (OperatingSystemType os in oss) {
            OperatingSystems.Add(os);
        }
        if (OperatingSystems.Count > 0) {
            SelectedOperatingSystemIndex = 0;
        }else {
            SelectedOperatingSystemIndex = -1;
        }
        
        Logger.LogInformation("Selected ISA changed to {arch}", isa);
        
        // update available templates
        Templates.Clear();
        Templates.Add(blankTemplate);
        Templates.AddRange(settingsService.TemplateSettings.Templates
            .Where(x => x.Architecture == isa));
        OnPropertyChanged(nameof(Templates));
        SelectedTemplateIndex = -1;
        SelectedTemplateIndex = 0;
    }
    
    partial void OnSelectedTemplateIndexChanged(int value) {
        if (value < 0 || value >= Templates.Count) {
            return;
        }
        Template template = Templates[value];
        if (template.IsBlank) {
            return;
        }
        // eh um template, modificar OS
        OperatingSystemType? os = OperatingSystemManager
            .GetAvailableOperatingSystems()
            .FirstOrDefault(x => x.CompatibleArchitecture == Isas[SelectedIsaIndex] 
                                && x.Identifier == template.OperatingSystemIdentifier);
        if (os is null) {
            Logger.LogWarning("Could not find a registered operating system that matches conditions");
            return;
        }

        int osIndex = OperatingSystems.IndexOf(os.Value);
        if (osIndex < 0) {
            Logger.LogWarning("Could not find Template operating system from loaded ones");
            return;
        }
        Logger.LogInformation("Changing OS because template uses a custom one");
        SelectedOperatingSystemIndex = osIndex;
    }

    [RelayCommand]
    private async Task NewProjectEnd() {
        string path = SanitizeProjectPath(NewProjectPath);
        string name = SanitizeProjectName(NewProjectName);
        string projectFilePath = Path.Combine(path, name, name+".asmproj");
        Template template = Templates[SelectedTemplateIndex];
        OperatingSystemType os = OperatingSystems[SelectedOperatingSystemIndex];
        Architecture isa = Isas[SelectedIsaIndex];
        ProjectFile project = await projectService.CreateProjectAsync(projectFilePath, name, os, isa, template);
        projectService.SetCurrentProject(project);
        IsCreatingProject = false;
        if(!projectSelectionTask.Task.IsCompleted) {
            projectSelectionTask.SetResult(true);
        }
    }
    
    [RelayCommand]
    private async Task OpenProjectDialog() {
        ProjectSelectionView? view = GetView();
        if (view is null) {
            return;
        }
        if (!view.StorageProvider.CanOpen) {
            Logger.LogError("FilePicker nao eh suportado nessa plataforma!");
            return;
        }
        
        IReadOnlyList<IStorageFile> result = await view.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = ProjectResources.SelectProjectPickerValue,
            AllowMultiple = false,
            SuggestedFileName = "project.asmproj",
            FileTypeFilter = [
                new FilePickerFileType(ProjectResources.AsmProjectsValue) {
                    Patterns = [ "*.asmproj" ],
                }
            ]
        });

        if (result.Count != 1) {
            return;
        }

        string path = result[0].Path.AbsolutePath;
        await OpenProject(path.ToFilePath());
    }
    
    [RelayCommand]
    private async Task OpenProject(PathObject path) {
        if (path.Extension != ".asmproj") {
            // esse check nao precisaria, mas melhor garantir
            return;
        }

        ProjectFile? project = await projectService.OpenProject(path);
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
    
    public void OverrideTaskCompletion() {
        Cancelled = true;
        if(!projectSelectionTask.Task.IsCompleted) {
            projectSelectionTask.SetResult(true);
        }
    }
}