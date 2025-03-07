using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;

namespace SAAE.Editor.ViewModels;

public partial class ProjectViewModel : BaseViewModel {

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
                break;
            case ProjectNodeType.Folder:
                node.ContextOptions = [
                    new ContextOption(node) {
                        Name = "New Folder",
                        Command = AddFolderCommand
                    },
                    new ContextOption(node) {
                        Name = "New File",
                        Command = AddFileCommand
                    }
                ];
                break;
            case ProjectNodeType.AssemblyFile:
                node.ContextOptions = [
                    new ContextOption(node) {
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
        Console.WriteLine($"Selected node: {value.Name}");
        // open file
    }

    [RelayCommand(CanExecute = nameof(CanAddFolder))]
    private void AddFolder(ProjectNode node) {
        ProjectNode folder = new() {
            Name = "folder",
            Children = [],
            Type = ProjectNodeType.Folder,
            Id = Guid.NewGuid()
        };
        fileService.RegisterNode(node, folder);
        SetCommands(folder);
    }

    private bool CanAddFolder(ProjectNode node) {
        return !node.IsEffectiveReadOnly;
    }

    [RelayCommand(CanExecute = nameof(CanAddFile))]
    private void AddFile(ProjectNode node) {

        if (node.Type != ProjectNodeType.Folder) {
            return;
        }
        
        ProjectNode file = new() {
            Name = "file.asm",
            Children = [],
            Type = ProjectNodeType.AssemblyFile,
            Id = Guid.NewGuid()
        };
        fileService.RegisterNode(node,file);
        SetCommands(file);
    }

    private bool CanAddFile(ProjectNode node) {
        return !node.IsEffectiveReadOnly;
    }

    [RelayCommand(CanExecute = nameof(CanSetEntryPoint))]
    private void SetEntryPoint(ProjectNode node) {
        ProjectFile? project = projectService.GetCurrentProject();
        Debug.Assert(project != null, "project != null (SetEntryPoint)");
        project.EntryFile = fileService.GetRelativePath(node.Id);
        projectService.SaveProject();
    }

    private bool CanSetEntryPoint(ProjectNode node) {
        Console.WriteLine("CanSetEntryPoint = "+!node.IsEffectiveReadOnly);
        return !node.IsEffectiveReadOnly;
    }
    
    [RelayCommand]
    private void RemoveNode(ProjectNode node) {
        
    }
}

