using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;

namespace SAAE.Editor.Services;

public class FileService {

    private readonly Guid stdlibCategoryId = Guid.Parse("408494E4-76DD-434C-97FF-6C40A4E9ED27");
    private readonly Guid projectCategoryId = Guid.Parse("C03D266B-3D00-486A-9517-D8A2F3065C53");
    
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    
    public FileService() {
        
    }
    
    public List<ProjectNode> GetProjectTree() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return [];
        }

        List<ProjectNode> nodes = [];
        
        if (project.IncludeStandardLibrary) {
            nodes.Add(GetStdLibNode());
        }

        List<ProjectNode> projectFiles = GetFolderNodes(project.ProjectDirectory);
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

    private ProjectNode GetStdLibNode() {
        var root = new ProjectNode {
            Name = Localization.ProjectResources.StdLibValue,
            Id = stdlibCategoryId,
            Type = ProjectNodeType.Category
        };
        
        // TODO: realmente pegar os arquivos da stdlib
        // talvez baixar do github? mais uma branch com
        // a std lib em cada ISA(risc-v, mips, etc)

        return root;
    }

    private List<ProjectNode> GetFolderNodes(string folder) {
        IEnumerable<string> entries = Directory.EnumerateFileSystemEntries(folder);
        List<ProjectNode> nodes = [];
        foreach (string entry in entries) {
            bool isFile = File.Exists(entry);
            bool isDirectory = Directory.Exists(entry);
            if (isFile && !isDirectory) {
                // arquivo
                nodes.Add(new ProjectNode {
                    Name = Path.GetFileName(entry),
                    Type = ProjectNodeType.AssemblyFile,
                    Id = Guid.NewGuid()
                });
            }else if(!isFile && isDirectory) {
                string folderName = new DirectoryInfo(entry).Name;
                nodes.Add(new ProjectNode {
                    Name = folderName,
                    Type = ProjectNodeType.Folder,
                    // recursao abaixo
                    Children = new ObservableCollection<ProjectNode>(GetFolderNodes(entry)),
                    Id = Guid.NewGuid()
                });
            }
            else {
                Console.WriteLine("Uma entrada nao eh nem arquivo nem pasta! Ignorando: " + entry);
            }
        }

        return nodes;
    }
}