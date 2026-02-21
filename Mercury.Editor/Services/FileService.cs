using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Editor.Extensions;
using Mercury.Editor.Models;
using Mercury.Editor.Models.Compilation;
using Mercury.Editor.Models.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.Services;

public class FileService : BaseService<FileService> {

    public static Guid StdLibCategoryId => Guid.Parse("408494E4-76DD-434C-97FF-6C40A4E9ED27");
    public static Guid ProjectCategoryId => Guid.Parse("C03D266B-3D00-486A-9517-D8A2F3065C53");
    
    private readonly ProjectService projectService;
    private readonly SettingsService settingsService;
    
    private readonly Dictionary<Guid, PathObject> relativePaths = [];
    private readonly Dictionary<Guid, ProjectNode> nodeAcceleration = [];
    private readonly Dictionary<Guid, bool> isStdlibNode = [];
    private ProjectNode? entryPoint;

    public FileService(ProjectService projectService, SettingsService settingsService) {
        this.projectService = projectService;
        this.settingsService = settingsService;
    }
    
    private void ResetCache() {
        relativePaths.Clear();
        nodeAcceleration.Clear();
        isStdlibNode.Clear();
    }
    
    /// <summary>
    /// (Re)Builds the project file tree and its associated structures.
    /// </summary>
    /// <returns></returns>
    public List<ProjectNode> GetProjectTree() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return [];
        }

        // reset node caches
        ResetCache();
        
        List<ProjectNode> nodes = [];
        
        if (project.IncludeStandardLibrary) {
            StandardLibrary? stdlib = settingsService.StdLibSettings.GetCompatibleLibrary(project);
            if (stdlib is null) {
                Logger.LogWarning("Nao encontrei standard library compativel com o projeto");
            }
            else {
                ProjectNode stdlibNode = GetStdLibNode(stdlib); 
                nodes.Add(stdlibNode);
                nodeAcceleration[StdLibCategoryId] = stdlibNode;
                relativePaths[StdLibCategoryId] = stdlib.Path;
            }
        }

        List<ProjectNode> projectFiles = GetFolderNodes(project.ProjectDirectory + project.SourceDirectory, "".ToDirectoryPath());
        ProjectNode projectCategoryNode = new() {
            Name = Localization.ProjectResources.ProjectFilesValue,
            Type = ProjectNodeType.Category,
            Children = new ObservableCollection<ProjectNode>(projectFiles),
            Id = ProjectCategoryId,
            IsExpanded = GetIsNodeOpen(ProjectCategoryId)
        };
        foreach (ProjectNode file in projectFiles) {
            file.ParentReference = new WeakReference<ProjectNode>(projectCategoryNode);
        }
        nodes.Add(projectCategoryNode);

        relativePaths[ProjectCategoryId] = project.ProjectDirectory + project.SourceDirectory;
        nodeAcceleration[ProjectCategoryId] = projectCategoryNode;

        return nodes;
    }

    public PathObject GetRelativePath(Guid nodeId) {
        if (nodeId == StdLibCategoryId || nodeId == ProjectCategoryId) {
            return default;
        }
        bool result = relativePaths.TryGetValue(nodeId, out PathObject relativePath);
        return !result ? default : relativePath;
    }

    public PathObject GetAbsolutePath(Guid nodeId) {
        PathObject relative = GetRelativePath(nodeId);
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return default;
        }

        if (isStdlibNode[nodeId]) {
            StandardLibrary? stdlib = settingsService.StdLibSettings.GetCompatibleLibrary(project);
            if (stdlib is null) {
                Logger.LogError("Nao encontrei uma stdlib compativel com o projeto");
                return default;
            }
            PathObject stdlibpath = stdlib.Path;
            Debug.Assert(stdlibpath.IsAbsolute, "Caminho da StdLib nas configs nao era absoluto.");
            return stdlibpath + relative;
        }
        return project.ProjectDirectory + project.SourceDirectory + relative;
    }

    public void RegisterNode(ProjectNode father, ProjectNode node) {
        PathObject fatherPath = relativePaths[father.Id];
        PathObject relativePath = default;
        if (father.Type == ProjectNodeType.Folder) {
            // node eh subdir de father
            if (node.Type == ProjectNodeType.Folder) {
                relativePath = fatherPath.Folder(node.Name);
            }else if (node.Type == ProjectNodeType.AssemblyFile) {
                relativePath = fatherPath.File(node.Name);
            }
        }else if (father.Type == ProjectNodeType.Category) {
            // esta na root do projeto
            if (node.Type == ProjectNodeType.AssemblyFile) {
                relativePath = node.Name.ToFilePath();
            }else if (node.Type == ProjectNodeType.Folder) {
                relativePath = node.Name.ToDirectoryPath();
            }
        }
        else{
            // soh eh filho logico, o path de node eh o dir de father
            if (father.ParentReference!.TryGetTarget(out ProjectNode? grandfather)) {
                relativePath = grandfather.Type is ProjectNodeType.Folder or ProjectNodeType.Category ?
                    relativePaths[grandfather.Id].Folder(node.Name) : relativePaths[grandfather.Id].File(node.Name);
            }
        }

        if (relativePath == default) {
            Logger.LogError("Could not figure out relative path for new registered node, using default. This" +
                            "may cause issues later.");
        }
        
        if (node.Id == Guid.Empty) {
            Logger.LogWarning("Tried registering new node with empty id. Generating a new id for {path}", relativePath.ToString());
            node.Id = GetIdFromPath(relativePath, false);
        }
        
        relativePaths[node.Id] = relativePath;
        nodeAcceleration[node.Id] = node;
        isStdlibNode[node.Id] = false;

        node.ParentReference = new WeakReference<ProjectNode>(father);
        father.Children.Add(node);

        // ordenar filhos por ordem alfabetica
        father.Children = new ObservableCollection<ProjectNode>(father.Children.OrderBy(x => x.Name));
    }

    public void UnregisterNode(ProjectNode node, bool first = true) {
        if (node.Id == Guid.Empty) {
            return;
        }

        relativePaths.Remove(node.Id);
        isStdlibNode.Remove(node.Id);
        nodeAcceleration.Remove(node.Id);
        foreach (NodeContextOption nodeContextOption in node.ContextOptions) {
            nodeContextOption.Dispose();
        }
        node.ContextOptions.Clear();

        if (first) {
            if (node.ParentReference!.TryGetTarget(out ProjectNode? parent)) {
                parent.Children.Remove(node);
            }else {
                Logger.LogWarning("Couldn't get reference to parent of {node} when deleting", node.Name);
            }
        }

        foreach (ProjectNode child in node.Children) {
            UnregisterNode(child, false);
        }
        node.Children.Clear();
    }

    public void MoveNode(ProjectNode node, ProjectNode newFather) {
        if (node.ParentReference!.TryGetTarget(out ProjectNode? oldFather)) {
            oldFather.Children.Remove(node);
        }
        else {
            return;
        }
        newFather.Children.Add(node);
        newFather.Children = new ObservableCollection<ProjectNode>(newFather.Children.OrderBy(x => x.Name));
        RecomputeRelativePaths(node, newFather);
        node.ParentReference = new WeakReference<ProjectNode>(newFather);
    }

    private void RecomputeRelativePaths(ProjectNode node, ProjectNode newFather) {
        PathObject old = GetAbsolutePath(node.Id);
        PathObject fatherPath = relativePaths[newFather.Id];
        if (newFather.Type == ProjectNodeType.Folder) {
            // node eh subdir de father
            if (node.Type == ProjectNodeType.Folder) {
                relativePaths[node.Id] = fatherPath.Folder(node.Name);
            }else if (node.Type == ProjectNodeType.AssemblyFile) {
                relativePaths[node.Id] = fatherPath.File(node.Name);
            }
        }else if (newFather.Type == ProjectNodeType.Category) {
            // esta na root do projeto
            if (node.Type == ProjectNodeType.AssemblyFile) {
                relativePaths[node.Id] = node.Name.ToFilePath();
            }else if (node.Type == ProjectNodeType.Folder) {
                relativePaths[node.Id] = node.Name.ToDirectoryPath();
            }
        }
        else{
            // soh eh filho logico, o path de node eh o dir de father
            if (newFather.ParentReference!.TryGetTarget(out ProjectNode? grandfather)) {
                relativePaths[node.Id] = grandfather.Type is ProjectNodeType.Folder or ProjectNodeType.Category ?
                relativePaths[grandfather.Id].Folder(node.Name) : relativePaths[grandfather.Id].File(node.Name);
            }
        }

        PathObject newPath = GetAbsolutePath(node.Id);
        WeakReferenceMessenger.Default.Send(new FileMoveMessage {
            OldPath = old,
            NewPath = newPath
        });

        foreach (ProjectNode child in node.Children) {
            RecomputeRelativePaths(child, node);
        }
    }

    public bool IsEntryPoint(Guid nodeId) {
        PathObject path = GetAbsolutePath(nodeId);
        ProjectFile? proj = projectService.GetCurrentProject();
        if (proj is null) return false;
        return path == proj.ProjectDirectory + proj.SourceDirectory + proj.EntryFile;
    }

    public ProjectNode GetNode(Guid nodeId)
    {
        return nodeAcceleration[nodeId];
    }
    
    /// <summary>
    /// Creates a <see cref="CompilationInput"/> object with all the
    /// files that need to be compiled
    /// </summary>
    /// <returns></returns>
    public CompilationInput CreateCompilationInput()
    {
        ProjectFile? project = projectService.GetCurrentProject();
        if(project is null) {
            return new CompilationInput();
        }
        List<CompilationFile> files = [];
        PathObject entryFile = project.EntryFile;
        foreach ((Guid id, PathObject path) in relativePaths)
        {
            if (nodeAcceleration[id].Type != ProjectNodeType.AssemblyFile)
            {
                continue;
            }

            files.Add(new CompilationFile(
                filepath: GetAbsolutePath(id),
                entryPoint: path.Equals(entryFile)));
        }

        if (!files.Exists(x => x.IsEntryPoint))
        {
            Debug.Fail("Nao havia nenhum arquivo no projeto registrado que fosse igual o entryPoint!");
        }
        return new CompilationInput
        {
            Files = files
        };
    }

    private ProjectNode GetStdLibNode(StandardLibrary stdlib) {
        var root = new ProjectNode {
            Name = Localization.ProjectResources.StdLibValue,
            Id = StdLibCategoryId,
            Type = ProjectNodeType.Category,
            IsReadOnly = true,
            IsExpanded = GetIsNodeOpen(StdLibCategoryId)
        };

        List<ProjectNode> children = GetFolderNodes(stdlib.Path, "".ToDirectoryPath(), isStdLib: true);
        foreach (ProjectNode child in children) {
            child.ParentReference = new WeakReference<ProjectNode>(root);
            child.IsReadOnly = true;
        }
        root.Children.AddRange(children.OrderBy(x => x.Name));
        // TODO: contabilizar outras arquiteturas

        return root;
    }

    private List<ProjectNode> GetFolderNodes(PathObject folder, PathObject currentPath, ProjectNode parentReference = null!,
        bool isStdLib = false) {
        List<ProjectNode> nodes = [];
        ProjectFile proj = projectService.GetCurrentProject()!;
        foreach (PathObject entry in folder) {
            string name = entry.Name;
            Guid nodeId = GetIdFromPath(
                entry.IsDirectory ? currentPath.Folder(name) : currentPath.File(name), isStdLib);
            ProjectNodeType type = entry is { IsFile: true, Extension: ".asm" or ".s" }
                ? ProjectNodeType.AssemblyFile
                : entry.IsFile
                    ? ProjectNodeType.UnknownFile
                    : ProjectNodeType.Folder;
            WeakReference<ProjectNode> parent = new(parentReference);
            bool isEntryPoint = type == ProjectNodeType.AssemblyFile
                                && folder.File(name) == proj.ProjectDirectory + proj.SourceDirectory + proj.EntryFile;
            bool isOutputFolder = entry.IsDirectory && entry == proj.ProjectDirectory + proj.OutputPath;
            if (isOutputFolder) {
                Logger.LogInformation("Skipped output folder: {folder}", entry.ToString());
                continue;
            }
            
            ProjectNode node = new() {
                Name = name,
                Type = type, 
                Id = nodeId,
                IsExpanded = GetIsNodeOpen(nodeId),
                ParentReference = parent,
                IsEntryPoint = isEntryPoint,
            };
            nodes.Add(node);
            if (isEntryPoint) {
                entryPoint = node;
            }

            if (type == ProjectNodeType.Folder) {
                node.Children = new ObservableCollection<ProjectNode>(GetFolderNodes(
                    folder: folder.Folder(name),
                    currentPath: currentPath.Folder(name), 
                    parentReference: node,
                    isStdLib: isStdLib));
            }
            
            // update caches
            relativePaths[node.Id] = entry.IsFile ? currentPath.File(node.Name) : currentPath.Folder(node.Name);
            nodeAcceleration[node.Id] = node;
            isStdlibNode[node.Id] = isStdLib;
        }
        return nodes;
    }

    public void SetNewEntryPoint(Guid id) {
        ProjectFile? project = projectService.GetCurrentProject();
        Debug.Assert(project != null, "project != null (SetEntryPoint)");
        entryPoint?.IsEntryPoint = false;
        project.EntryFile = GetRelativePath(id);
        entryPoint = nodeAcceleration[id];
        entryPoint.IsEntryPoint = true;
        projectService.SaveProject();
    }

    private Guid GetIdFromPath(PathObject path, bool isStdLib) {
        if (isStdLib) {
            path = "stdlib".ToDirectoryPath() + path;
        }
        string pathStr = path.ToString();
        byte[] bytes = Encoding.ASCII.GetBytes(pathStr);
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(bytes,hash);
        return new Guid(hash[..16]);
    }

    private bool GetIsNodeOpen(Guid nodeId) {
        List<OpenProjectNode> nodes = projectService.GetCurrentProject()!.VisualSettings.OpenProjectNodes;
        OpenProjectNode? node = nodes.Find(x => x.NodeId == nodeId);
        if (node is null) {
            node = new OpenProjectNode { NodeId = nodeId, IsOpen = false };
            nodes.Add(node);
            projectService.SaveProject(); // save many times? hopefully only happens on first time opening.
            return false;
        }
        return node.IsOpen;
    }
}
