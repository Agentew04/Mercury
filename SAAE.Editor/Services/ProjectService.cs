using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.ViewModels;
using SAAE.Engine;

namespace SAAE.Editor.Services;

/// <summary>
/// A service to read, write and manage projects.
/// </summary>
public class ProjectService {

    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();
    private ProjectFile? currentProject = null;

    private static bool PathEquals(string path1, string path2) {
        string normalize(string p) {
            return Path.GetFullPath(p)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(normalize(path1), normalize(path2), comparison);
    }
    
    /// <summary>
    /// Returns the path to the most recent projects.
    /// </summary>
    /// <returns></returns>
    public List<ProjectFile> GetRecentProjects() {
        List<UserPreferences.ProjectAccess> recent = settingsService.Preferences.RecentProjects;
        // usar foreach normal?
        // azar, nao deve ter performance ruim nem rodar toda hora
        return recent
            .Select(x => (Project: ReadProject(x.Path), LastAccess: x.LastOpen))
            .Where(x => x.Project is not null)
            .ForEach(x => x.Project!.LastAccessed = x.LastAccess)
            .Select(x => x.Project!)
            .ToList();
    }

    private static ProjectFile? ReadProject(string path) {
        if (!path.EndsWith(".asmproj")) {
            return null;
        }
        
        if (!File.Exists(path)) {
            return null;
        }
        
        using var reader = XmlReader.Create(path, new XmlReaderSettings {
            Async = true
        });
        
        System.Xml.Serialization.XmlSerializer serializer = new(typeof(ProjectFile));
        ProjectFile? project;
        try {
            if (serializer.Deserialize(reader) is not ProjectFile proj) {
                return null;
            }

            project = proj;
        }
        catch (InvalidOperationException) {
            return null;
        }
        if (project.ProjectVersion > ProjectFile.LatestProjectVersion) {
            Console.WriteLine("A versao do projeto eh maior que a versao suportada! Atualize o programa.");
            return null;
        }
        
        project.OperatingSystem = OperatingSystemManager.GetAvailableOperatingSystems()
            .First(x => x.Name == project.OperatingSystemName);

        project.ProjectPath = path;
        return project;
    }
    
    private static void WriteProject(ProjectFile project) {
        if (!project.ProjectPath.EndsWith(".asmproj")) {
            throw new ArgumentException("Project path must end with .asmproj");
        }
        if (!Directory.Exists(project.ProjectDirectory)) {
            Directory.CreateDirectory(project.ProjectDirectory);
        }
        using var writer = XmlWriter.Create(project.ProjectPath, new XmlWriterSettings() {
            Indent = true,
            IndentChars = "    "
        });
        
        project.OperatingSystemName = project.OperatingSystem.Name;
        
        System.Xml.Serialization.XmlSerializer serializer = new(typeof(ProjectFile));
        serializer.Serialize(writer, project);
    }

    
    public async Task<ProjectFile> CreateProjectAsync(string path, string name, OperatingSystemType os, Architecture isa) {
        ProjectFile project = new() {
            ProjectName = name,
            ProjectPath = path,
            EntryFile = "src/main.asm",
            OperatingSystem = os,
            OperatingSystemName = os.Name,
            ProjectVersion = ProjectFile.LatestProjectVersion,
            IncludeStandardLibrary = true,
            Architecture = isa
        };
        WriteProject(project);
        // get directory from filepath
        Directory.CreateDirectory(Path.Combine(project.ProjectDirectory, "src"));
        await File.WriteAllTextAsync(Path.Combine(project.ProjectDirectory, project.EntryFile), "");
        SetRecentAccess(project);
        await settingsService.SaveSettings();
        return project;
    }

    public async Task<ProjectFile?> OpenProject(string path) {
        ProjectFile? project = ReadProject(path);
        if (project is null) {
            return null;
        }
        UpdateProject(project);
        SetRecentAccess(project);
        await settingsService.SaveSettings();
        return project;
    }
    
    private void SetRecentAccess(ProjectFile project) {
        DateTime accessTime = DateTime.Now;
        project.LastAccessed = accessTime;
        settingsService.Preferences.RecentProjects.RemoveAll(x => PathEquals(x.Path, project.ProjectPath));
        settingsService.Preferences.RecentProjects.Add(new UserPreferences.ProjectAccess(project.ProjectPath, accessTime));
    }

    private void UpdateProject(ProjectFile projectFile) {
        if(projectFile.ProjectVersion == ProjectFile.LatestProjectVersion) {
            return;
        }
        
        if(projectFile.ProjectVersion == 1) {
            // atualizar para a versao 2 quando houver
        }
    }
    
    public ProjectFile? GetCurrentProject() {
        return currentProject;
    }
    
    public void SetCurrentProject(ProjectFile? project) {
        currentProject = project;
    }
}