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
using SAAE.Editor.Views.CodeView;
using SAAE.Engine.Common;
using YamlDotNet.Serialization;
using Path = System.IO.Path;

namespace SAAE.Editor.ViewModels.Code;

public partial class ProjectViewModel : BaseViewModel<ProblemsViewModel, ProblemsView> {

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
                    new NodeContextOption {
                        Resource = () => ProjectResources.NewFileContextMenuValue,
                        Command = AddFileCommand
                    },
                    new NodeContextOption {
                        Resource = () => ProjectResources.NewFolderContextMenuValue,
                        Command = AddFolderCommand
                    },
                ];
                break;
            case ProjectNodeType.Folder:
                node.ContextOptions = [
                    new NodeContextOption {
                        Resource = () => ProjectResources.NewFileContextMenuValue,
                        Command = AddFileCommand
                    },
                    new NodeContextOption {
                        Resource = () => ProjectResources.NewFolderContextMenuValue,
                        Command = AddFolderCommand
                    },
                    new NodeContextOption {
                        Resource = () => ProjectResources.DeleteFolderContextMenuValue,
                        Command = RemoveNodeCommand
                    }
                ];
                break;
            case ProjectNodeType.AssemblyFile:
                node.ContextOptions = [
                    new NodeContextOption() {
                        Resource = () => ProjectResources.SetEntryPointContextMenuValue,
                        Command = SetEntryPointCommand
                    },
                    new NodeContextOption {
                        Resource = () => ProjectResources.DeleteFileContextMenuValue,
                        Command = RemoveNodeCommand
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

    // partial void OnSelectedNodeChanged(ProjectNode? value) {
    //     if (value is null) {
    //         return;
    //     }
    //     Logger.LogInformation($"Selected node: {value.Name}. Type: {value.Type}");
    //
    //     if (value.Type == ProjectNodeType.AssemblyFile) {
    //         Logger.LogInformation("Abrindo arquivo " + value.Name);
    //         WeakReferenceMessenger.Default.Send(new FileOpenMessage
    //         {
    //             ProjectNode = value
    //         });
    //     }
    // }

    public void SelectNode(ProjectNode value) {
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
    private async Task AddFolder(ProjectNode? node) {
        if(node is null) return;

        // only add folders to categories or folders
        if (node.Type != ProjectNodeType.Category && node.Type == ProjectNodeType.Folder) {
            return;
        }
        
        TextPopupResult result = await WeakReferenceMessenger.Default.Send(new RequestTextPopupMessage() {
            Title = ProjectResources.NewFolderModalTitleValue,
            IsCancellable = true,
            Watermark = ProjectResources.NewFolderModalWatermarkValue
        });
        if (result.IsCancelled) {
            return;
        }
        
        // sanitize folder name
        char[] invalidChars = Path.GetInvalidPathChars();
        string foldername = result.Result.Sanitize(invalidChars);
        
        ProjectNode folder = new() {
            Name = foldername,
            Children = [],
            Type = ProjectNodeType.Folder,
            Id = Guid.NewGuid()
        };
        fileService.RegisterNode(node, folder);
        SetCommands(folder);
        PathObject path = fileService.GetAbsolutePath(folder.Id);
        Directory.CreateDirectory(path.ToString());
    }

    private bool CanAddFolder(ProjectNode? node) {
        if (node is null) return false;
        if (node.Type == ProjectNodeType.Category) {
            return node.Id == fileService.ProjectCategoryId;
        }
        return !node.IsEffectiveReadOnly;
    }

    [RelayCommand(CanExecute = nameof(CanAddFile))]
    private async Task AddFile(ProjectNode? node) {
        if (node is null) {
            return;
        }
        
        // only add files to categories and folders
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
        
        // sanitize file name
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string filename = result.Result.Sanitize(invalidChars);
        
        string ext = Path.GetExtension(result.Result);
        ProjectNode file = new() {
            Name = filename,
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
        if (node.Type == ProjectNodeType.Category) {
            return node.Id == fileService.ProjectCategoryId;
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

        ProjectFile project = projectService.GetCurrentProject()!;
        PathObject entryPath = project.ProjectDirectory + project.SourceDirectory + project.EntryFile;
        PathObject nodePath = fileService.GetAbsolutePath(node.Id);
        bool isEntryPoint = entryPath == nodePath;
        return !node.IsEffectiveReadOnly && node.Type == ProjectNodeType.AssemblyFile && !isEntryPoint;
    }
    
    [RelayCommand(CanExecute = nameof(CanRemoveNode))]
    private async Task RemoveNode(ProjectNode? node) {
        if (node is null) {
            return;
        }

        if (node.IsEffectiveReadOnly) {
            return;
        }

        // confirm action prompt
        string titleFmt = node.Type == ProjectNodeType.Folder
            ? ProjectResources.DeleteFolderConfirmModalValue
            : ProjectResources.DeleteFileConfirmModalValue;
        string title = string.Format(titleFmt, node.Name);
        BoolPopupResult result = await WeakReferenceMessenger.Default.Send(new RequestBoolPopupMessage {
            IsCancellable = true,
            Title = title
        });
        if (result.IsCancelled || !result.Result) {
            return;
        }
        
        Logger.LogInformation("Deleting node: {node}", node.Name);
        
        WeakReferenceMessenger.Default.Send(new FileDeleteMessage {
            ProjectNode = node
        });

        string path = fileService.GetAbsolutePath(node.Id).ToString();
        switch (node.Type) {
            case ProjectNodeType.AssemblyFile:
                if (File.Exists(path)) {
                    File.Delete(path);
                }
                break;
            case ProjectNodeType.Folder:
                if(Directory.Exists(path)) {
                    Directory.Delete(path, true);
                }
                break;
        }
        
        fileService.UnregisterNode(node);
        if (Nodes.Remove(node)) {
            // coloca aqui soh pra garantir. vai que 
            Logger.LogWarning("Removed a root node from the project tree");
        }
    }

    private bool CanRemoveNode(ProjectNode? node) {
        if (node is null) {
            return false;
        }

        if (node.Type != ProjectNodeType.Folder && node.Type != ProjectNodeType.AssemblyFile) {
            return false;
        }

        ProjectFile project = projectService.GetCurrentProject()!;
        PathObject entryPath = project.ProjectDirectory + project.SourceDirectory + project.EntryFile;
        PathObject nodePath = fileService.GetAbsolutePath(node.Id);

        return !node.IsEffectiveReadOnly && nodePath != entryPath;
    }

    #region Drag and Drop

    private ProjectNode? draggedNode;
    
    public void StartDrag(ProjectNode node) {
        draggedNode = node;
    }
    
    public bool IsNodeValidForDrop(ProjectNode? node) {
        if (node is null) return false;
        return !node.IsEffectiveReadOnly && node.Type == ProjectNodeType.Category ||
               node.Type == ProjectNodeType.Folder && node.Id != draggedNode?.Id;
    }

    public void Drop(ProjectNode? node, ProjectNode? target) {
        if (node is null || target is null) {
            return;
        }

        PathObject oldPath = fileService.GetAbsolutePath(node.Id);
        fileService.MoveNode(node, target);
        PathObject newPath = fileService.GetAbsolutePath(node.Id);
        if (node.Type == ProjectNodeType.AssemblyFile) {
            File.Move(oldPath.ToString(), newPath.ToString());
        }else if (node.Type == ProjectNodeType.Folder) {
            Directory.Move(oldPath.ToString(), newPath.ToString());
        }
        else {
            Logger.LogWarning("Tried to move an immovable node!");
        }
        Logger.LogInformation("Drop node {node} into {target}", node.Name, target.Name);
        draggedNode = null;
    }

    public void ForceEndDrag() {
        // as vezes o Drop pode nao ser chamado
        draggedNode = null;
    }

    public bool IsEntryPoint(ProjectNode node) => fileService.IsEntryPoint(node.Id);


    #endregion

}

