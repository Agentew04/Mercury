using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;

namespace SAAE.Editor.ViewModels;

public partial class ProjectViewModel : BaseViewModel {

    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();

    public ProjectViewModel() {
        Nodes = [
            // stdlib
            new ProjectNode{
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
            new ProjectNode {
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
    }
    
    [ObservableProperty] private ObservableCollection<ProjectNode> nodes = [];

    [ObservableProperty] private ProjectNode selectedNode = null!;

    partial void OnSelectedNodeChanged(ProjectNode value) {
        Console.WriteLine($"Selected node: {value.Name}");
        // open file
    }
}

