using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public ProjectViewModel() {
        // temp
        List<ProjectNode> nodes = [
            new() {
                Name = "Standard Library",
                Type = ProjectNodeType.Category,
                Children = [
                    new ProjectNode {
                        Name = "printf.asm",
                        Type = ProjectNodeType.AssemblyFile
                    },
                    new ProjectNode {
                        Name = "scanf.asm",
                        Type = ProjectNodeType.AssemblyFile
                    }
                ]
            },
            // user files

            new() {
                Name = "Project Files",
                Type = ProjectNodeType.Category,
                Children = [
                    new ProjectNode {
                        Name = "src",
                        Type = ProjectNodeType.Folder,
                        Children = [
                            new ProjectNode {
                                Name = "main.asm",
                                Type = ProjectNodeType.AssemblyFile
                            }
                        ]
                    },
                    new ProjectNode {
                        Name = "image.bmp",
                        Type = ProjectNodeType.UnknownFile
                    }
                ]
            }
        ];
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
                    new ContextOption() {
                        Name = "New Folder",
                        Command = AddFolderCommand
                    },
                    new ContextOption() {
                        Name = "New File",
                        Command = AddFileCommand
                    }
                ];
                break;
            case ProjectNodeType.AssemblyFile:
                node.ContextOptions = [
                    new ContextOption() {
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

    [RelayCommand]
    private void AddFolder() {
        
    }

    [RelayCommand]
    private void AddFile() {
        ProjectNode? node = SelectedNode;
        
        Console.WriteLine("add file:" + node?.Name ?? "null");
    }

    [RelayCommand]
    private void SetEntryPoint() {
        ProjectNode node = SelectedNode;
        Console.WriteLine("Set entry point: "+ node.Name);
        
    }
    
    [RelayCommand]
    private void RemoveNode() {
        
    }
}

