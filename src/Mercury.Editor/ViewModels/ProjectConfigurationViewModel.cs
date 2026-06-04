using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Editor.Models;
using Mercury.Editor.Services;
using Mercury.Editor.Views;
using Mercury.Engine.Common;
using Microsoft.Extensions.Logging;
using Mercury.Editor.Extensions;
using Mercury.Editor.Models.Messages;
using Mercury.Editor.Models.Modules;
using Mercury.Editor.Models.Modules.Properties;
using Mercury.Editor.Utils;

namespace Mercury.Editor.ViewModels;

public partial class ProjectConfigurationViewModel : BaseViewModel<ProjectConfigurationViewModel, ProjectConfiguration> {

    private readonly ProjectService projectService;

    [ObservableProperty] private string projectName = string.Empty;
    [ObservableProperty] private bool includeStdlib;
    public List<Architecture> AvailableArchs { get; } = [Architecture.Mips, Architecture.RiscV, Architecture.Arm];
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ApplyCommand))] private int selectedArchIndex;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ApplyCommand))] private ObservableCollection<string> availableOperatingSystems = [];
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ApplyCommand))] private string selectedOs = string.Empty;
    [ObservableProperty] private string srcDir = string.Empty;
    [ObservableProperty] private string outputDir = string.Empty;
    [ObservableProperty] private string outputFile = string.Empty;
    [ObservableProperty] private string entryFile = string.Empty;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ApplyCommand))] private ObservableCollectionEx<ModuleDescription>? moduleDescriptions;

    [ObservableProperty] private ObservableCollection<ProjectConfigurationModuleItem> availableNewModules = [];
    [NotifyCanExecuteChangedFor(nameof(AddModuleCommand))]
    [ObservableProperty] private int selectedNewModuleIndex = -1;

    public ProjectConfigurationViewModel(ProjectService projectService) {
        this.projectService = projectService;
    }

    [SuppressMessage("Trimming", "IL2072:Target parameter argument does not satisfy \'DynamicallyAccessedMembersAttribute\' in call to target method. The return value of the source method does not have matching annotations.")]
    public void Load() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return;
        }

        ProjectName = project.ProjectName;
        IncludeStdlib = project.IncludeStandardLibrary;
        SelectedArchIndex = AvailableArchs.IndexOf(project.Architecture);
        AvailableOperatingSystems.Clear();
        AvailableOperatingSystems.AddRange(
                OperatingSystemManager.GetAvailableOperatingSystems()
                    .Where(x => x.CompatibleArchitecture == project.Architecture)
                    .Select(x => x.Name)
            );
        // Logger.LogInformation("Count: {cnt} OS", AvailableOperatingSystems.Count);
        SelectedOs = project.OperatingSystem.Name;
        SrcDir = project.SourceDirectory.ToString();
        OutputDir = project.OutputPath.ToString();
        OutputFile = project.OutputFile.ToString();
        EntryFile = project.EntryFile.ToString();

        // existing modules
        ModuleDescriptions = [];
        ModuleDescriptions.ItemPropertyChanged += (s, e) => {
            ObservableCollectionEx<ModuleDescription> tmp = ModuleDescriptions;
            ModuleDescriptions = null!;
            ModuleDescriptions = tmp;
        };
        ModuleDescriptions.AddRange(project.InstalledModules);
        
        // new modules
        IReadOnlyList<Type> moduleTypes = ModuleDescription.GetAvailableModules();
        foreach (Type moduleType in moduleTypes) {
            ModuleDescription? moduleInstance = (ModuleDescription?)Activator.CreateInstance(moduleType);
            if (moduleInstance is null) {
                Logger.LogError("There was a type registered in ModuleDescription.GetAvailableModules() that does not " +
                                "inherit from ModuleDescription. Could not register it. Type was: {type}", moduleType.FullName);
                continue;
            }

            ProjectConfigurationModuleItem item = new() {
                Name = moduleInstance.ModuleName,
                Type = moduleType,
                IsEnabled = true
            };
            AvailableNewModules.Add(item);
        }
        RefreshEnabledNewModules();
        Logger.LogInformation("Project has {installed} installed modules and of these, {active} are active.", ModuleDescriptions.Count, ModuleDescriptions.Count(x => x.Active));
    }
    
    partial void OnSelectedArchIndexChanged(int value) {
        AvailableOperatingSystems.Clear();
        AvailableOperatingSystems.AddRange(
            OperatingSystemManager.GetAvailableOperatingSystems()
                .Where(x => x.CompatibleArchitecture == AvailableArchs[value])
                .Select(x => x.Name)
        );
        try {
            SelectedOs = AvailableOperatingSystems[0];
        }
        catch (Exception) {
            SelectedOs = string.Empty;
        }
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Apply() {
        ApplyProject();
        GetView()?.Close();
    }

    public bool CanApply() {
        bool validOs = AvailableOperatingSystems.Count > 0 && SelectedOs != string.Empty;
        if (ModuleDescriptions is null) {
            return false;
        }
        // must have a cpu and a memory.
        bool hasCpu = ModuleDescriptions.Where(x => x is ICpuModuleDescription)
            .Any(x => x is { Active: true });
        bool hasMemory = ModuleDescriptions.Where(x => x is MemoryModuleDescription)
            .Any(x => x is MemoryModuleDescription { Active: true, BlockCount: > 0, BlockSize: > 0 });
        bool validModules = hasCpu && hasMemory;
        
        return validOs && validModules;
    }

    private void ApplyProject() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return;
        }

        project.ProjectName = ProjectName;
        bool rebuildProjectTree = project.IncludeStandardLibrary != IncludeStdlib;
        project.IncludeStandardLibrary = IncludeStdlib;
        if (rebuildProjectTree) {
            // tell services that we need to rebuild the project tree items
            WeakReferenceMessenger.Default.Send<ProjectTreeInvalidationMessage>();
        }
        project.Architecture = AvailableArchs[SelectedArchIndex];
        try {
            project.OperatingSystem = OperatingSystemManager.GetAvailableOperatingSystems()
                .Where(x => x.CompatibleArchitecture == project.Architecture)
                .First(x => x.Name == SelectedOs /*AvailableOperatingSystems[SelectedOsIndex]*/);
            project.OperatingSystemName = project.OperatingSystem.Name;
        }
        catch (InvalidOperationException) {
            Logger.LogError("Nome do SO selecionado nao existe para esta arquitetura!");
        }
        project.SourceDirectory = SrcDir.ToDirectoryPath();
        project.OutputPath = OutputDir.ToDirectoryPath();
        project.OutputFile = OutputFile.ToFilePath();
        project.EntryFile = EntryFile.ToFilePath();
        project.InstalledModules.Clear();
        project.InstalledModules.AddRange(ModuleDescriptions ?? []);
        
        projectService.SaveProject();
    }

    [RelayCommand]
    private void DeleteModule(ModuleDescription moduleDescription) {
        ModuleDescriptions?.Remove(moduleDescription);
        RefreshEnabledNewModules();
    }

    [RelayCommand(CanExecute = nameof(CanAddModule))]
    private void AddModule() {
        // add module to list
        ProjectConfigurationModuleItem item = AvailableNewModules[SelectedNewModuleIndex];
        ModuleDescription? description = (ModuleDescription?)Activator.CreateInstance(item.Type);
        if (description is null) {
            return;
        }
        SelectedNewModuleIndex = -1;
        ModuleDescriptions?.Add(description);
        RefreshEnabledNewModules();
    }
    
    private bool CanAddModule() {
        bool indexValid = SelectedNewModuleIndex != -1
                     && SelectedNewModuleIndex < AvailableNewModules.Count;
        if (!indexValid) {
            return false;
        }
        ProjectConfigurationModuleItem item = AvailableNewModules[SelectedNewModuleIndex];
        return item.IsEnabled;
    }

    private void RefreshEnabledNewModules() {
        foreach (ProjectConfigurationModuleItem item in AvailableNewModules) {
            item.IsEnabled = ModuleDescriptions?.All(x => x.GetType() != item.Type) ?? false;
        }
    }
}

public partial class ProjectConfigurationModuleItem : ObservableObject {
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private bool isEnabled;
    
    public required Type Type { get; set; }
}