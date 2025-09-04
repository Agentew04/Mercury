using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Localization;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using SAAE.Editor.Views;
using SAAE.Engine;
using SAAE.Engine.Common;

namespace SAAE.Editor.ViewModels;

public partial class ProjectConfigurationViewModel : BaseViewModel<ProjectConfigurationViewModel, ProjectConfiguration> {

    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();

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
        Logger.LogInformation("Count: {cnt} OS", AvailableOperatingSystems.Count);
        SelectedOs = project.OperatingSystem.Name;
        SrcDir = project.SourceDirectory.ToString();
        OutputDir = project.OutputPath.ToString();
        OutputFile = project.OutputFile.ToString();
        EntryFile = project.EntryFile.ToString();
        
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
        Logger.LogInformation("Arch: {arch}; SO Count: {cnt}; Selected: {sel}", AvailableArchs[value], AvailableOperatingSystems.Count, SelectedOs);
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Apply() {
        ApplyProject();
        GetView()?.Close();
    }

    public bool CanApply() {
        return AvailableOperatingSystems.Count > 0 && SelectedOs != string.Empty; //SelectedOsIndex != -1;
    }

    private void ApplyProject() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return;
        }

        project.ProjectName = ProjectName;
        project.IncludeStandardLibrary = IncludeStdlib;
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
        
        projectService.SaveProject();
    }
}