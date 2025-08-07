using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Models;
using SAAE.Engine;
using SAAE.Engine.Common;

namespace SAAE.Editor.Services;

/// <summary>
/// A service to read, write and manage projects.
/// </summary>

public class ProjectService : BaseService<ProjectService> {

    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();
    private ProjectFile? currentProject;
    
    /// <summary>
    /// Returns the path to the most recent projects.
    /// </summary>
    /// <returns></returns>
    public List<ProjectFile> GetRecentProjects() {
        List<UserPreferences.ProjectAccess> recent = settingsService.Preferences.RecentProjects;
        // usar foreach normal?
        // azar, nao deve ter performance ruim nem rodar toda hora
        return recent
            .Select(x => (Project: ReadProject(x.Path.ToFilePath()), LastAccess: x.LastOpen))
            .Where(x => x.Project is not null)
            .ForEachExt(x => x.Project!.LastAccessed = x.LastAccess)
            .Select(x => x.Project!)
            .ToList();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private ProjectFile? ReadProject(PathObject path) {
        if (path.Extension != ".asmproj") {
            return null;
        }
        
        if (!File.Exists(path.ToString())) {
            return null;
        }
        
        using var reader = XmlReader.Create(path.ToString(), new XmlReaderSettings());
        
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
            Logger.LogError("A versao do projeto eh maior que a versao suportada! Atualize o programa.");
            return null;
        }
        
        project.OperatingSystem = OperatingSystemManager.GetAvailableOperatingSystems()
            .First(x => x.Name == project.OperatingSystemName);

        project.ProjectPath = path;
        return project;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", 
        Justification = "ProjectFile class is annotated with [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]")]
    private static void WriteProject(ProjectFile project) {
        if (project.ProjectPath.Extension != ".asmproj") {
            throw new ArgumentException("Project path must end with .asmproj");
        }
        Directory.CreateDirectory(project.ProjectDirectory.ToString());
        using var writer = XmlWriter.Create(project.ProjectPath.ToString(), new XmlWriterSettings() {
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
            ProjectPath = path.ToFilePath(),
            EntryFile = "main.asm".ToFilePath(),
            SourceDirectory = "src/".ToDirectoryPath(),
            OutputPath = "bin/".ToDirectoryPath(),
            OutputFile = $"{name}.elf".ToFilePath(),
            OperatingSystem = os,
            OperatingSystemName = os.Name,
            ProjectVersion = ProjectFile.LatestProjectVersion,
            IncludeStandardLibrary = true,
            Architecture = isa
        };
        WriteProject(project);
        // create folder structure
        string srcDir = project.ProjectDirectory.Append(project.SourceDirectory).ToString();
        string binDir = project.ProjectDirectory.Append(project.OutputPath).ToString();
        string entryFile = project.ProjectDirectory.Append(project.SourceDirectory).Append(project.EntryFile).ToString();
        Logger.LogInformation("Creating directory: {dir}", srcDir);
        Directory.CreateDirectory(srcDir);
        Logger.LogInformation("Creating directory: {dir}", binDir);
        Directory.CreateDirectory(binDir);
        Logger.LogInformation("Creating file: {file}", entryFile);
        await File.WriteAllTextAsync(entryFile, "");
        SetRecentAccess(project);
        await settingsService.SaveSettings();
        return project;
    }

    public async Task<ProjectFile?> OpenProject(PathObject path) {
        ProjectFile? project = ReadProject(path);
        if (project is null) {
            return null;
        }

        if (UpdateProject(project)) {
            WriteProject(project);
        }
        SetRecentAccess(project);
        await settingsService.SaveSettings();
        return project;
    }
    
    private void SetRecentAccess(ProjectFile project) {
        DateTime accessTime = DateTime.Now;
        project.LastAccessed = accessTime;
        settingsService.Preferences.RecentProjects.RemoveAll(x => x.Path.ToFilePath().Equals(project.ProjectPath));
        settingsService.Preferences.RecentProjects.Add(new UserPreferences.ProjectAccess(project.ProjectPath.ToString(), accessTime));
    }

    private bool UpdateProject(ProjectFile projectFile) {
        if(projectFile.ProjectVersion == ProjectFile.LatestProjectVersion) {
            return false;
        }
        
        if(projectFile.ProjectVersion == 1) {
            projectFile.OutputPath = "bin/".ToDirectoryPath();
            projectFile.OutputFile = "main.exe".ToFilePath();
            projectFile.ProjectVersion = 2;
            Logger.LogInformation("Projeto atualizado para versao 2");
        }

        if (projectFile.ProjectVersion == 2) {
            projectFile.SourceDirectory = "src/".ToDirectoryPath();
            Logger.LogInformation("Projeto atualizado para versao 3");
        }
        // preencher com novas versoes quando houver
        return true;
    }
    
    public ProjectFile? GetCurrentProject() {
        return currentProject;
    }
    
    public void SetCurrentProject(ProjectFile? project) {
        currentProject = project;
    }
    
    public void SaveProject() {
        if (currentProject is null) {
            return;
        }
        WriteProject(currentProject);
    }
}