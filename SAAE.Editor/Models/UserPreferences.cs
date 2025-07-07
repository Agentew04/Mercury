using System;
using System.Collections.Generic;
using System.Globalization;
using SAAE.Editor.Extensions;

namespace SAAE.Editor.Models;

public class UserPreferences {
    
    /// <summary>
    /// The latest version available for the configuration file.
    /// </summary>
    public const int LatestConfigVersion = 4;
    
    /// <summary>
    /// The version of the configuration file.
    /// If less than <see cref="LatestConfigVersion"/>, a converter
    /// will be used to update the file on app start.
    /// </summary>
    public int ConfigVersion { get; set; } = LatestConfigVersion;
    
    /// <summary>
    /// The path to the compiler and linker executables.
    /// </summary>
    public string CompilerPath { get; set; } = "";

    /// <summary>
    /// The path to the standard library of the system.
    /// </summary>
    public string StdLibPath { get; set; } = "";
    
    /// <summary>
    /// The current language of the application.
    /// </summary>
    public CultureInfo Language { get; set; } = new("pt-BR");
    
    /// <summary>
    /// A list with the most recent project opened by the user
    /// and the time it was last opened.
    /// </summary>
    public List<ProjectAccess> RecentProjects { get; set; } = [];

    public record ProjectAccess(PathObject Path, DateTime LastOpen);
}