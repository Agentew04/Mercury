using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Mercury.Editor.Models;

public class UserPreferences {
    
    /// <summary>
    /// The latest version available for the configuration file.
    /// </summary>
    public const int LatestConfigVersion = 5;
    
    /// <summary>
    /// The version of the configuration file.
    /// If less than <see cref="LatestConfigVersion"/>, a converter
    /// will be used to update the file on app start.
    /// </summary>
    public int ConfigVersion { get; set; } = LatestConfigVersion;
    
    [JsonIgnore] public static string CompilerFileName => $"clang{(OperatingSystem.IsWindows() ? ".exe" : string.Empty)}";
    [JsonIgnore] public static string AssemblerFileName => $"llvm-mc{(OperatingSystem.IsWindows() ? ".exe" : string.Empty)}";
    [JsonIgnore] public static string LinkerFileName => $"ld.lld{(OperatingSystem.IsWindows() ? ".exe" : string.Empty)}";

    /// <summary>
    /// The current language of the application.
    /// </summary>
    public CultureInfo Language { get; set; } = new("pt-BR");
    
    public DateTime LastOnlineCheck { get; set; }
    
    public TimeSpan OnlineCheckFrequency { get; set; }

    /// <summary>
    /// The string key of the preferred theme variant.
    /// </summary>
    public string Theme { get; set; } = string.Empty;

    /// <summary>
    /// A list with the most recent project opened by the user
    /// and the time it was last opened.
    /// </summary>
    public List<ProjectAccess> RecentProjects { get; set; } = [];

    public record ProjectAccess(string Path, DateTime LastOpen);
}

