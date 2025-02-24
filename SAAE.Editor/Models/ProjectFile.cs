using System;
using System.IO;
using System.Xml.Serialization;

namespace SAAE.Editor.Models;

/// <summary>
/// A class that represents a project file 
/// </summary>
[XmlRoot("Project")]
public class ProjectFile {

    /// <summary>
    /// The latest version available for project files. 
    /// </summary>
    public const int LatestProjectVersion = 1;
    
    /// <summary>
    /// The project version that this file is using. If
    /// less than <see cref="LatestProjectVersion"/>, a converter
    /// will be used to update the file.
    /// </summary>
    [XmlAttribute("Version")]
    public int ProjectVersion { get; set; } = 1;
    
    /// <summary>
    /// The path to the project file. It is understood that
    /// the directory the project path is in is the root directory
    /// of the project.
    /// </summary>
    [XmlIgnore]
    public string ProjectPath { get; set; } = "";

    /// <summary>
    /// Returns the base directory of the project.
    /// </summary>
    public string ProjectDirectory => Path.GetDirectoryName(ProjectPath) ?? "";

    /// <summary>
    /// The user given name for the project.
    /// </summary>
    [XmlElement("ProjectName")]
    public string ProjectName { get; set; } = "";

    /// <summary>
    /// Wether to include the custom IDE assembly library or not.
    /// </summary>
    [XmlElement("IncludeStdLib")]
    public bool IncludeStandardLibrary { get; set; } = true;

    /// <summary>
    /// What instruction set to use for the project.
    /// </summary>
    [XmlElement("InstructionSet")]
    public string InstructionSet { get; set; } = "mips";
    
    /// <summary>
    /// The string that represents the name of the operating system
    /// used to simulate syscalls.
    /// </summary>
    [XmlElement("OperatingSystem")]
    public string OperatingSystemName { get; set; } = "mars";

    /// <summary>
    /// The main entry point file of the project. It is this file
    /// that will be injected a __start label and globl directive
    /// </summary>
    [XmlElement("EntryFile")]
    public string EntryFile { get; set; } = "src/main.asm";
    
    /// <summary>
    /// A timestamp of when the project was last accessed.
    /// Not serialized because is saved on the settings file.
    /// </summary>
    [XmlIgnore]
    public DateTime LastAccessed { get; set; }
}