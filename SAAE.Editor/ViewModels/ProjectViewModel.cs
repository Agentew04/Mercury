using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Services;

namespace SAAE.Editor.ViewModels;

public partial class ProjectViewModel : BaseViewModel {

    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();

    public ProjectViewModel() {
        Nodes = [
            // stdlib
            new Node{
                Name = "Standard Library",
                Type = NodeType.Category,
                Children = [
                    new Node {
                        Name = "printf.asm",
                        Type = NodeType.AssemblyFile
                    },
                    new Node {
                        Name = "scanf.asm",
                        Type = NodeType.AssemblyFile
                    }
                ]
            },
            // user files
            new Node {
                Name = "Project Files",
                Type = NodeType.Category,
                Children = [
                    new Node {
                        Name = "src",
                        Type = NodeType.Folder,
                        Children = [
                            new Node {
                                Name = "main.asm",
                                Type = NodeType.AssemblyFile
                            }
                        ]
                    },
                    new Node {
                        Name = "image.bmp",
                        Type = NodeType.UnknownFile
                    }
                ]
            }
        ];
    }
    
    [ObservableProperty] private ObservableCollection<Node> nodes = [];
}

public enum NodeType {
    None,
    Category,
    Folder,
    AssemblyFile,
    UnknownFile
}

public partial class Node : ObservableObject {

    [ObservableProperty]
    private string name = string.Empty;
    
    [ObservableProperty]
    private NodeType type = NodeType.None;
    
    [ObservableProperty]
    private ObservableCollection<Node> children = [];
}