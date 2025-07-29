using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Localization;
using SAAE.Editor.Models;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using SAAE.Engine.Common;

namespace SAAE.Editor.ViewModels.Code;

public partial class ProjectViewModel : BaseViewModel<ProblemsViewModel> {

    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    private readonly FileService fileService = App.Services.GetRequiredService<FileService>();

    public ProjectViewModel() {
        List<ProjectNode> nodes = fileService.GetProjectTree();
        foreach (ProjectNode node in nodes) {
            SetCommands(node);
        }

        Nodes = new ObservableCollection<ProjectNode>(nodes);
    }

    private void SetCommands(ProjectNode node) {
        switch (node.Type) {
            case ProjectNodeType.Category:
                node.ContextOptions = [
                    new ContextOption {
                        Name = "New File",
                        Command = AddFileCommand
                    }
                ];
                break;
            case ProjectNodeType.Folder:
                node.ContextOptions = [
                    new ContextOption {
                        Name = "New Folder",
                        Command = AddFolderCommand
                    },
                    new ContextOption {
                        Name = "New File",
                        Command = AddFileCommand
                    }
                ];
                break;
            case ProjectNodeType.AssemblyFile:
                node.ContextOptions = [
                    new ContextOption {
                        Name = "Set Entry Point",
                        Command = SetEntryPointCommand
                    }
                ];
                break;
            case ProjectNodeType.None:
            case ProjectNodeType.UnknownFile:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(node), node.Type, null);
        }
        
        // recurse
        foreach (ProjectNode child in node.Children) {
            SetCommands(child);
        }
    }
    
    [ObservableProperty] private ObservableCollection<ProjectNode> nodes = [];

    [ObservableProperty] private ProjectNode selectedNode = null!;

    partial void OnSelectedNodeChanged(ProjectNode value) {
        Logger.LogInformation($"Selected node: {value.Name}. Type: {value.Type}");

        if (value.Type == ProjectNodeType.AssemblyFile) {
            Logger.LogInformation("Abrindo arquivo " + value.Name);
            WeakReferenceMessenger.Default.Send(new FileOpenMessage
            {
                ProjectNode = value
            });
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddFolder))]
    private void AddFolder(ProjectNode? node) {
        if(node is null) return;
        ProjectNode folder = new() {
            Name = "folder",
            Children = [],
            Type = ProjectNodeType.Folder,
            Id = Guid.NewGuid()
        };
        fileService.RegisterNode(node, folder);
        SetCommands(folder);
    }

    private bool CanAddFolder(ProjectNode? node) {
        if (node is null) return false;
        return !node.IsEffectiveReadOnly;
    }

    [RelayCommand(CanExecute = nameof(CanAddFile))]
    private async Task AddFile(ProjectNode? node) {
        if (node is null) {
            return;
        }
        if (node.Type != ProjectNodeType.Folder && node.Type != ProjectNodeType.Category) {
            return;
        }

        TextPopupResult result = await WeakReferenceMessenger.Default.Send(new RequestTextPopupMessage() {
            Title = ProjectResources.NewFileModalTitleValue,
            IsCancellable = true,
            Watermark = ProjectResources.NewFileModalWatermarkValue
        });
        if (result.IsCancelled) {
            return;
        }
        string ext = System.IO.Path.GetExtension(result.Result);
        ProjectNode file = new() {
            Name = result.Result,
            Children = [],
            Type = ext is ".asm" or ".s" ? ProjectNodeType.AssemblyFile : ProjectNodeType.UnknownFile,
            Id = Guid.NewGuid()
        };
        fileService.RegisterNode(node,file);
        SetCommands(file);
        PathObject path = fileService.GetAbsolutePath(file.Id);
        await File.WriteAllTextAsync(path.ToString(), "");
    }

    private bool CanAddFile(ProjectNode? node) {
        if (node is null) return false;
        if (node.Type == ProjectNodeType.Category && node.Id == fileService.ProjectCategoryId) {
            return true;
        }
        if (node.Type == ProjectNodeType.Category) {
            return false;
        }

        return !node.IsEffectiveReadOnly;
    }

    [RelayCommand(CanExecute = nameof(CanSetEntryPoint))]
    private void SetEntryPoint(ProjectNode? node) {
        if (node is null) {
            return;
        }
        ProjectFile? project = projectService.GetCurrentProject();
        Debug.Assert(project != null, "project != null (SetEntryPoint)");
        project.EntryFile = fileService.GetRelativePath(node.Id);
        projectService.SaveProject();
    }

    private bool CanSetEntryPoint(ProjectNode? node) {
        if (node is null) {
            return false;
        }
        return !node.IsEffectiveReadOnly;
    }
    
    [RelayCommand]
    private void RemoveNode(ProjectNode? node) {
        if(node is null) return;
    }
}

