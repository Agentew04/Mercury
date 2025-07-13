using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SAAE.Editor.Extensions;
using SAAE.Engine;
using SAAE.Engine.Common;

namespace SAAE.Editor.Models;

/// <summary>
/// A class that represents a project file 
/// </summary>
[XmlRoot("Project")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
// atributo acima eh necessario caso um membro nao seja referenciado. ele vai 
// ser deletado pelo trimmer.
public class ProjectFile {
    
    /// <summary>
    /// The latest version available for project files. 
    /// </summary>
    public const int LatestProjectVersion = 3;
    
    /// <summary>
    /// The project version that this file is using. If
    /// less than <see cref="LatestProjectVersion"/>, a converter
    /// will be used to update the file.
    /// </summary>
    [XmlAttribute("Version")]
    public int ProjectVersion { get; set; } = LatestProjectVersion;

    #region Implicit Properties

    /// <summary>
    /// The path to the project file. It is understood that
    /// the directory the project path is in is the root directory
    /// of the project.
    /// </summary>
    [XmlIgnore]
    public PathObject ProjectPath { get; set; }

    /// <summary>
    /// Returns the base directory of the project.
    /// </summary>
    [XmlIgnore]
    public PathObject ProjectDirectory => ProjectPath.Path();

    #endregion

    #region Project Options

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
    [XmlElement("Architecture")]
    public Architecture Architecture { get; set; } = Architecture.Mips;
    
    /// <summary>
    /// The operating system that the project will use.
    /// </summary>
    [XmlIgnore]
    public OperatingSystemType OperatingSystem { get; set; }
    
    /// <summary>
    /// The name of the operating system that the project will
    /// use.
    /// </summary>
    [XmlElement("OperatingSystem")]
    public string OperatingSystemName { get; set; } = "";

    #endregion

    #region Files and Paths

    /// <summary>
    /// The folder where the source files are located. This is relative to
    /// <see cref="ProjectDirectory"/>.
    /// </summary>
    [XmlElement("SourceDirectory")]
    public PathObject SourceDirectory { get; set; } = "src/".ToDirectoryPath();
    
    /// <summary>
    /// The main entry point file of the project. It is this file
    /// that will be injected a '__start' label and .globl directive.
    /// </summary>
    /// <remarks>Relative to <see cref="SourceDirectory"/></remarks>
    [XmlElement("EntryFile")]
    public PathObject EntryFile { get; set; } = "main.asm".ToFilePath();
    
    
    /// <summary>
    /// A timestamp of when the project was last accessed.
    /// Not serialized because is saved on the settings file.
    /// </summary>
    [XmlIgnore]
    public DateTime LastAccessed { get; set; }

    /// <summary>
    /// The folder where the binaries will be placed. Relative
    /// to <see cref="ProjectDirectory"/>
    /// </summary>
    [XmlElement("OutputPath")]
    public PathObject OutputPath { get; set; } = "bin/".ToDirectoryPath();
    
    /// <summary>
    /// The name of the generated binary file.
    /// </summary>
    /// <remarks>This is relative to <see cref="OutputPath"/>!</remarks>
    [XmlElement("OutputFile")]
    public PathObject OutputFile { get; set; } = "output.bin".ToFilePath();
    
    #endregion
    

}