using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;

namespace SAAE.Editor.Services;

public class FileService {

    private readonly Guid stdlibCategoryId = Guid.Parse("408494E4-76DD-434C-97FF-6C40A4E9ED27");
    private readonly Guid projectCategoryId = Guid.Parse("C03D266B-3D00-486A-9517-D8A2F3065C53");
    
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    
    private readonly Dictionary<Guid, ProjectNodeType> nodeTypes = [];
    private readonly Dictionary<Guid, string> relativePaths = [];
    
    public FileService() {
        
    }

    private void ResetCache() {
        nodeTypes.Clear();
        relativePaths.Clear();
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

        List<ProjectNode> projectFiles = GetFolderNodes(project.ProjectDirectory, "");
        // Aviso: a localizacao dos nos de categoria NAO vao ser atualizadas
        // na troca de lingua desse modo!
        nodes.Add(new ProjectNode {
            Name = Localization.ProjectResources.ProjectFilesValue,
            Type = ProjectNodeType.Category,
            Children = new ObservableCollection<ProjectNode>(projectFiles),
            Id = projectCategoryId
        });
        
        return nodes;
    }

    public string GetRelativePath(Guid nodeId) {
        if (nodeId == stdlibCategoryId || nodeId == projectCategoryId) {
            return "";
        }
        bool result = relativePaths.TryGetValue(nodeId, out string? relativePath);
        if (!result) {
            return "";
        }
        Debug.Assert(relativePath != null);
        return relativePath;
    }

    public string GetAbsolutePath(Guid nodeId) {
        string relative = GetRelativePath(nodeId);
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return "";
        }

        // Path.Combine nao junta se o segundo for 'absoluto' (comeca com barra)
        if (relative.StartsWith('\\') || relative.StartsWith('/')) {
            relative = relative[1..];
        }
        string absPath = Path.Combine(project.ProjectDirectory, relative); 
        return absPath;
    }

    public void RegisterNode(ProjectNode father, ProjectNode node) {
        if (node.Id == Guid.Empty) {
            node.Id = Guid.NewGuid();
        }
        
        string fatherPath = relativePaths[father.Id];
        if (father.Type == ProjectNodeType.Folder) {
            // node eh subdir de father
            relativePaths[node.Id] = fatherPath + Path.DirectorySeparatorChar + node.Name;
        }else if (father.Type == ProjectNodeType.Category) {
            // esta na root do projeto
            relativePaths[node.Id] = node.Name;
        }
        else{
            // soh eh filho logico, o path de node eh o dir de father
            string? dir = Path.GetDirectoryName(fatherPath);
            Debug.Assert(dir != null, "dir != null (RegisterNode)");
            relativePaths[node.Id] = dir + Path.DirectorySeparatorChar + node.Name;
        }

        nodeTypes[node.Id] = node.Type;

        node.ParentReference = new WeakReference<ProjectNode>(father);
        father.Children.Add(node);
        
        // ordenar filhos por ordem alfabetica
        father.Children = new ObservableCollection<ProjectNode>(father.Children.OrderBy(x => x.Name));
    }

    private ProjectNode GetStdLibNode() {
        var root = new ProjectNode {
            Name = Localization.ProjectResources.StdLibValue,
            Id = stdlibCategoryId,
            Type = ProjectNodeType.Category,
            IsReadOnly = true
        };

        var testfile = new ProjectNode() {
            Name = "scanf.asm",
            Id = Guid.NewGuid(),
            Type = ProjectNodeType.AssemblyFile,
        };
        testfile.ParentReference = new WeakReference<ProjectNode>(root);
        root.Children.Add(testfile);
        // TODO: realmente pegar os arquivos da stdlib
        // talvez baixar do github? mais uma branch com
        // a std lib em cada ISA(risc-v, mips, etc)

        return root;
    }

    private List<ProjectNode> GetFolderNodes(string folder, string currentPath) {
        IEnumerable<string> entries = Directory.EnumerateFileSystemEntries(folder);
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
                    Id = Guid.NewGuid()
                };
                nodes.Add(node);
            }else if (isFile && !isCodeExtension) {
                // arquivo aleatorio
                node = new ProjectNode {
                    Name = Path.GetFileName(entry),
                    Type = ProjectNodeType.UnknownFile,
                    Id = Guid.NewGuid()
                };
                nodes.Add(node);
            }else if(isDirectory) {
                string folderName = new DirectoryInfo(entry).Name;
                node = new ProjectNode {
                    Name = folderName,
                    Type = ProjectNodeType.Folder,
                    // recursao abaixo
                    Children = new ObservableCollection<ProjectNode>(GetFolderNodes(entry,
                        currentPath + Path.DirectorySeparatorChar + folderName)),
                    Id = Guid.NewGuid()
                };
                nodes.Add(node);
            }
            else {
                Console.WriteLine("Uma entrada nao eh nem arquivo nem pasta! Ignorando: " + entry);
            }

            if (node is null) {
                continue;
            }
            // cache node info
            relativePaths[node.Id] = currentPath + Path.DirectorySeparatorChar + node.Name;
            nodeTypes[node.Id] = node.Type;
        }

        return nodes;
    }
    
    
}
