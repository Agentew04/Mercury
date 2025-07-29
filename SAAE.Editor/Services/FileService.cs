using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AvaloniaEdit.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Models;
using SAAE.Editor.Models.Compilation;

namespace SAAE.Editor.Services;

public class FileService : BaseService<FileService> {

    public Guid StdLibCategoryId => Guid.Parse("408494E4-76DD-434C-97FF-6C40A4E9ED27");
    public Guid ProjectCategoryId => Guid.Parse("C03D266B-3D00-486A-9517-D8A2F3065C53");
    
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();
    
    private readonly Dictionary<Guid, ProjectNodeType> nodeTypes = [];
    private readonly Dictionary<Guid, PathObject> relativePaths = [];
    private readonly Dictionary<Guid, ProjectNode> nodeAcceleration = [];
    private readonly Dictionary<Guid, bool> isStdlibNode = [];
    private List<ProjectNode> internalTree = [];
    
    private void ResetCache() {
        nodeTypes.Clear();
        relativePaths.Clear();
        nodeAcceleration.Clear();
        isStdlibNode.Clear();
    }
    
    public List<ProjectNode> GetProjectTree() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return [];
        }

        // reset node caches
        ResetCache();
        
        List<ProjectNode> nodes = [];
        
        if (project.IncludeStandardLibrary) {
            nodes.Add(GetStdLibNode());
        }

        List<ProjectNode> projectFiles = GetFolderNodes(project.ProjectDirectory + project.SourceDirectory, "".ToDirectoryPath());
        // Aviso: a localizacao dos nos de categoria NAO vao ser atualizadas
        // na troca de lingua desse modo!
        // Resolver: #18
        nodes.Add(new ProjectNode {
            Name = Localization.ProjectResources.ProjectFilesValue,
            Type = ProjectNodeType.Category,
            Children = new ObservableCollection<ProjectNode>(projectFiles),
            Id = ProjectCategoryId
        });

        relativePaths[StdLibCategoryId] = settingsService.Preferences.StdLibPath.ToDirectoryPath();
        relativePaths[ProjectCategoryId] = project.ProjectDirectory + project.SourceDirectory;
        nodeTypes[StdLibCategoryId] = ProjectNodeType.Category;
        nodeTypes[ProjectCategoryId] = ProjectNodeType.Category;

        internalTree = nodes;
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
            PathObject stdlib = settingsService.Preferences.StdLibPath.ToDirectoryPath();
            Debug.Assert(stdlib.IsAbsolute, "Caminho da StdLib nas configs nao era absoluto.");
            return stdlib + relative;
        }
        return project.ProjectDirectory + project.SourceDirectory + relative;
    }

    public void RegisterNode(ProjectNode father, ProjectNode node) {
        if (node.Id == Guid.Empty) {
            node.Id = Guid.NewGuid();
        }
        
        PathObject fatherPath = relativePaths[father.Id];
        if (father.Type == ProjectNodeType.Folder) {
            // node eh subdir de father
            relativePaths[node.Id] = fatherPath.Folder(node.Name);
        }else if (father.Type == ProjectNodeType.Category) {
            // esta na root do projeto
            relativePaths[node.Id] = node.Name.ToFilePath();
        }
        else{
            // soh eh filho logico, o path de node eh o dir de father
            relativePaths[node.Id] = fatherPath.File(node.Name);
        }

        nodeTypes[node.Id] = node.Type;
        isStdlibNode[node.Id] = false;

        node.ParentReference = new WeakReference<ProjectNode>(father);
        father.Children.Add(node);
        
        // ordenar filhos por ordem alfabetica
        father.Children = new ObservableCollection<ProjectNode>(father.Children.OrderBy(x => x.Name));
    }

    public ProjectNode? GetNode(Guid nodeId)
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
        PathObject entryPoint = project.EntryFile;
        foreach ((Guid id, PathObject path) in relativePaths)
        {
            if (nodeTypes[id] != ProjectNodeType.AssemblyFile)
            {
                continue;
            }

            files.Add(new CompilationFile(
                filepath: GetAbsolutePath(id),
                entryPoint: path.Equals(entryPoint)));
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

    private ProjectNode GetStdLibNode() {
        var root = new ProjectNode {
            Name = Localization.ProjectResources.StdLibValue,
            Id = StdLibCategoryId,
            Type = ProjectNodeType.Category,
            IsReadOnly = true
        };

        List<ProjectNode> children = GetFolderNodes(settingsService.Preferences.StdLibPath.ToDirectoryPath(), "".ToDirectoryPath(), isStdLib: true);
        if (children.RemoveAll(x => x.Name == "version.json") != 1) {
            Debug.Fail("StandardLibrary nao tinha arquivo chamado version.json na root!");
        }
        foreach (ProjectNode child in children) {
            child.ParentReference = new WeakReference<ProjectNode>(root);
            child.IsReadOnly = true;
        }
        root.Children.AddRange(children);
        // TODO: contabilizar outras arquiteturas

        return root;
    }

    private List<ProjectNode> GetFolderNodes(PathObject folder, PathObject currentPath, ProjectNode parentReference = null!,
        bool isStdLib = false) {
        IEnumerable<string> entries = Directory.EnumerateFileSystemEntries(folder.ToString());
        List<ProjectNode> nodes = [];
        foreach (string entry in entries) {
            bool isFile = File.Exists(entry);
            bool isDirectory = Directory.Exists(entry);
            bool isCodeExtension = entry.EndsWith(".asm") || entry.EndsWith(".s");
            ProjectNode? node = null;
            if (isFile && isCodeExtension) {
                // arquivo assembly
                node = new ProjectNode {
                    Name = Path.GetFileName(entry),
                    Type = ProjectNodeType.AssemblyFile,
                    Id = Guid.NewGuid(),
                    ParentReference = new WeakReference<ProjectNode>(parentReference)
                };
                nodes.Add(node);
            }else if (isFile && !isCodeExtension) {
                // arquivo aleatorio
                node = new ProjectNode {
                    Name = Path.GetFileName(entry),
                    Type = ProjectNodeType.UnknownFile,
                    Id = Guid.NewGuid(),
                    ParentReference = new WeakReference<ProjectNode>(parentReference)
                };
                nodes.Add(node);
            }else if(isDirectory) {
                string folderName = new DirectoryInfo(entry).Name;

                // pula pasta de output bin/
                // isso n vai funcionar se o user mudar a bin pra uma nested!
                if (currentPath.Parts.Length == 0 && folderName == projectService.GetCurrentProject()!.OutputPath.Parts[0])
                {
                    continue;
                }
                
                node = new ProjectNode {
                    Name = folderName,
                    Type = ProjectNodeType.Folder,
                    Id = Guid.NewGuid(),
                    ParentReference = new WeakReference<ProjectNode>(parentReference)
                };
                node.Children = new ObservableCollection<ProjectNode>(GetFolderNodes(
                    folder: folder.Folder(Path.GetFileName(entry)),
                    currentPath: currentPath.Folder(folderName), 
                    parentReference: node,
                    isStdLib: isStdLib));
                nodes.Add(node);
            }
            else {
                Console.WriteLine("Uma entrada nao eh nem arquivo nem pasta! Ignorando: " + entry);
            }

            if (node is null) {
                continue;
            }
            // cache node info
            relativePaths[node.Id] = isFile ? currentPath.File(node.Name) : currentPath.Folder(node.Name);
            nodeTypes[node.Id] = node.Type;
            nodeAcceleration[node.Id] = node;
            isStdlibNode[node.Id] = isStdLib;
        }

        return nodes;
    }
}
