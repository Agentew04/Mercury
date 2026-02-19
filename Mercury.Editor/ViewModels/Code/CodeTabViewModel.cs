using System;
using System.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Editor.Extensions;
using Mercury.Editor.Models;
using Mercury.Editor.Services;
using Mercury.Editor.Views.CodeView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.ViewModels.Code;

public partial class CodeTabViewModel : BaseViewModel<CodeTabViewModel, CodeTabView> {
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();

    private const string GuideName = "codetab.guide";
    private const string ProjectName = "codetab.project";
    private const string ProblemsName = "codetab.problems";

    [ObservableProperty] private double guideColumnWidth;
    [ObservableProperty] private double projectViewColumnWidth;
    [ObservableProperty] private double problemsViewRowHeight;

    private CancellationTokenSource cts = new();

    public CodeTabViewModel() {
        DispatcherTimer.Run(SaveSizes, TimeSpan.FromMinutes(1), DispatcherPriority.Background);
    }

    public void LoadSizes() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            Logger.LogInformation("Tried loading preferred size with no project loaded.");
            return;
        }

        ProjectVisualSettings settings = project.VisualSettings;
        bool save = CreateOrGet(settings, GuideName, out int guideSize, 300)
                    | CreateOrGet(settings, ProjectName, out int projectSize, 300)
                    | CreateOrGet(settings, ProblemsName, out int problemsSize, 300);

        GuideColumnWidth = guideSize;
        ProjectViewColumnWidth = projectSize;
        ProblemsViewRowHeight = problemsSize;
        
        if (save) {
            projectService.SaveProject();
        }
    }

    private bool SaveSizes() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            Logger.LogInformation("Tried loading preferred size with no project loaded.");
            return true;
        }

        ProjectVisualSettings settings = project.VisualSettings;
        bool save = CreateOrSet(settings, GuideName, (int)GuideColumnWidth)
                    | CreateOrSet(settings, ProjectName, (int)ProjectViewColumnWidth)
                    | CreateOrSet(settings, ProblemsName, (int)ProblemsViewRowHeight);
        
        if (save) {
            Logger.LogInformation("Saving layout information on code tab");
            projectService.SaveProject();
        }
        return !cts.IsCancellationRequested;
    }

    public void OnUnload() {
        SaveSizes();
        cts.Cancel();
        cts.Dispose();
    }
    
    private static bool CreateOrGet(ProjectVisualSettings settings, string name, out int size, int defaultSize) {
        ElementSize? elem = settings.ElementSizes.Find(x => x.Name == name);
        if (elem is null) {
            elem = new ElementSize {
                Name = name,
                Size = defaultSize
            };
            settings.ElementSizes.Add(elem);
            size = defaultSize;
            return true;
        }
        size = elem.Size;
        return false;
    }

    private static bool CreateOrSet(ProjectVisualSettings settings, string name, int size) {
        ElementSize? elem = settings.ElementSizes.Find(x => x.Name == name);
        bool save = false;
        if (elem is null) {
            save = true;
            elem = new ElementSize {
                Name = name,
                Size = size
            };
            settings.ElementSizes.Add(elem);
        }

        if (Math.Approximately(elem.Size, size)) return save;
        save = true;
        elem.Size = size;
        return save;
    }
}