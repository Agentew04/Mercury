using System;
using System.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Editor.Extensions;
using Mercury.Editor.Models;
using Mercury.Editor.Services;
using Mercury.Editor.Views.ExecuteView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.ViewModels.Execute;

public partial class ExecuteViewModel : BaseViewModel<ExecuteViewModel, ExecuteView> {

    private readonly ProjectService projectService;
    
    private const string RegistersName = "executetab.registers";
    private const string LabelsName = "executetab.labels";
    private const string RamName = "executetab.ram";
    private const string IoName = "executetab.io";
    
    [ObservableProperty] private double registersViewColumnWidth;
    [ObservableProperty] private double labelsViewColumnWidth;
    [ObservableProperty] private double ramRowHeight;
    [ObservableProperty] private double ioRowHeight;
    
    private readonly CancellationTokenSource cts = new();

    public ExecuteViewModel(ProjectService projectService) {
        this.projectService = projectService;
        DispatcherTimer.Run(SaveSizes, TimeSpan.FromMinutes(1), DispatcherPriority.Background);
    }
    
    public void LoadSizes() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            Logger.LogInformation("Tried loading sizes without a loaded project.");
            return;
        }
        
        ProjectVisualSettings settings = project.VisualSettings;
        // use single | to avoid short-circuit(didn't test if it is really needed with the 'out' parameter) 
        bool createdNew = CreateOrGet(settings, RegistersName, out int registersSize, 300)
                          | CreateOrGet(settings, LabelsName, out int labelsSize, 300)
                          | CreateOrGet(settings, RamName, out int ramSize, 500)
                          | CreateOrGet(settings, IoName, out int ioSize, 200);
       
        RegistersViewColumnWidth = registersSize;
        LabelsViewColumnWidth = labelsSize;
        RamRowHeight = ramSize;
        IoRowHeight = ioSize;

        // save if necessary
        if (createdNew) {
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
        bool save = CreateOrSet(settings, RegistersName, (int)RegistersViewColumnWidth)
                    | CreateOrSet(settings, LabelsName, (int)LabelsViewColumnWidth)
                    | CreateOrSet(settings, RamName, (int)RamRowHeight)
                    | CreateOrSet(settings, IoName, (int)IoRowHeight);

        if (save) {
            Logger.LogInformation("Saving layout from Execute Tab");
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