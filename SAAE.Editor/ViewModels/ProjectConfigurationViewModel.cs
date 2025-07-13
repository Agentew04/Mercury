using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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

public partial class ProjectConfigurationViewModel : BaseViewModel<ProjectConfigurationViewModel> {

    public ProjectConfiguration View { get; set; } = null!;

    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();

    [ObservableProperty] private string projectName = string.Empty;
    [ObservableProperty] private bool includeStdlib;
    public List<Architecture> AvailableArchs { get; } = [Architecture.Mips, Architecture.RiscV, Architecture.Arm];
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ApplyCommand))] private int selectedArchIndex;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ApplyCommand))] private ObservableCollection<OperatingSystemType> availableOperatingSystems = [];
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ApplyCommand))] private int selectedOsIndex;
    [ObservableProperty] private string srcDir = string.Empty;
    [ObservableProperty] private string outputDir = string.Empty;
    [ObservableProperty] private string outputFile = string.Empty;
    [ObservableProperty] private string entryFile = string.Empty;
    public List<CultureInfo> AvailableLanguages { get; } = [..LocalizationManager.AvailableCultures];
    [ObservableProperty] private int selectedLanguageIndex;

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
            );
        SelectedOsIndex = AvailableOperatingSystems.IndexOf(project.OperatingSystem);
        if (SelectedOsIndex == -1) {
            SelectedOsIndex = 0;
        }
        SrcDir = project.SourceDirectory.ToString();
        OutputDir = project.OutputPath.ToString();
        OutputFile = project.OutputFile.ToString();
        EntryFile = project.EntryFile.ToString();
        SelectedLanguageIndex = AvailableLanguages.IndexOf(LocalizationManager.CurrentCulture);
    }
    
    partial void OnSelectedArchIndexChanged(int value) {
        AvailableOperatingSystems.Clear();
        AvailableOperatingSystems.AddRange(
            OperatingSystemManager.GetAvailableOperatingSystems()
                .Where(x => x.CompatibleArchitecture == AvailableArchs[SelectedArchIndex])
        );
        SelectedOsIndex = AvailableOperatingSystems.Count > 0 ? 0 : -1;
    }
    
    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Apply() {
        ApplyProject();
        ApplyPreferences();
        View.Close();
    }

    public bool CanApply() {
        return AvailableOperatingSystems.Count > 0 && SelectedOsIndex != -1;
    }

    private void ApplyProject() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return;
        }

        project.ProjectName = ProjectName;
        project.IncludeStandardLibrary = IncludeStdlib;
        project.Architecture = AvailableArchs[SelectedArchIndex];
        project.OperatingSystem = AvailableOperatingSystems[SelectedOsIndex];
        project.OperatingSystemName = project.OperatingSystem.Name;
        project.SourceDirectory = SrcDir.ToDirectoryPath();
        project.OutputPath = OutputDir.ToDirectoryPath();
        project.OutputFile = OutputFile.ToFilePath();
        project.EntryFile = EntryFile.ToFilePath();
        
        projectService.SaveProject();
    }

    private void ApplyPreferences() {
        if (!AvailableLanguages[SelectedLanguageIndex].Equals(LocalizationManager.CurrentCulture)) {
            Logger.LogInformation("Changing to culture: {culture}", AvailableLanguages[SelectedLanguageIndex]);
            LocalizationManager.CurrentCulture = AvailableLanguages[SelectedLanguageIndex];
        }
    }
}