using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Services;

/// <summary>
/// A service to read, write and manage projects.
/// </summary>
public class ProjectService {

    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();
    private ProjectFile? currentProject = null;

    /// <summary>
    /// Returns the path to the most recent projects.
    /// </summary>
    /// <returns></returns>
    public List<ProjectFile> GetRecentProjects() {
        List<(string path, DateTime lastOpen)> recent = settingsService.Preferences.RecentProjects;
        // usar foreach normal?
        // azar, nao deve ter performance ruim nem rodar toda hora
        return recent
            .Select(x => (Project: ReadProject(x.path), LastAccess: x.lastOpen))
            .Where(x => x.Project is not null)
            .ForEach(x => x.Project!.LastAccessed = x.LastAccess)
            .Select(x => x.Project!)
            .ToList();
    }

    private static ProjectFile? ReadProject(string path) {
        if (!path.EndsWith(".asmproj")) {
            return null;
        }
        
        using var reader = XmlReader.Create(path);
        
        System.Xml.Serialization.XmlSerializer serializer = new(typeof(ProjectFile));
        if (serializer.Deserialize(reader) is not ProjectFile project) {
            return null;
        }
        project.ProjectPath = path;
        return project;
    }
    
    private static void WriteProject(ProjectFile project) {
        if (!project.ProjectPath.EndsWith(".asmproj")) {
            throw new ArgumentException("Project path must end with .asmproj");
        }
        string? directory = Path.GetDirectoryName(project.ProjectPath);
        if (directory is null) {
            throw new ArgumentException("Project path must have a directory");
        }
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }
        using var writer = XmlWriter.Create(project.ProjectPath);
        
        System.Xml.Serialization.XmlSerializer serializer = new(typeof(ProjectFile));
        serializer.Serialize(writer, project);
    }

    
    public ProjectFile CreateProject(string path, string name, string os) {
        ProjectFile project = new() {
            ProjectName = name,
            ProjectPath = path,
            EntryFile = "src/main.asm",
            OperatingSystemName = os,
            ProjectVersion = ProjectFile.LatestProjectVersion,
            IncludeStandardLibrary = true
        };
        WriteProject(project);
        // get directory from filepath
        string baseDir = Path.GetDirectoryName(project.ProjectPath) ?? throw new ArgumentException("Project path must have a directory");
        Directory.CreateDirectory(Path.Combine(baseDir, "src"));
        File.WriteAllText(project.EntryFile, "");
        return project;
    }

    public ProjectFile? OpenProject(string path) {
        ProjectFile? project = ReadProject(path);
        if (project is null) {
            return null;
        }
        UpdateProject(project);
        DateTime accessTime = DateTime.Now;
        project.LastAccessed = accessTime;
        settingsService.Preferences.RecentProjects.RemoveAll(x => x.path == path);
        settingsService.Preferences.RecentProjects.Add((path, accessTime));
        return project;
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