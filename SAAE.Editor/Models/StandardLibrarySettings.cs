﻿using System.Collections.Generic;
using System.Text.Json.Serialization;
using SAAE.Editor.Extensions;
using SAAE.Engine;

namespace SAAE.Editor.Models;

public class StandardLibrarySettings {

    /// <summary>
    /// A list with all available libraries downloaded.
    /// </summary>
    public List<StandardLibrary> AvailableLibraries { get; set; } = [];

    public StandardLibrary? GetCompatibleLibrary(ProjectFile project) {
        // TODO: considerar sistema operacional tambem!
        return AvailableLibraries.Find(x => x.Architecture == project.Architecture);
    }
}

/// <summary>
/// Represents a standard library.
/// </summary>
public class StandardLibrary {
    /// <summary>
    /// The compatible architecture of this library.
    /// </summary>
    [JsonPropertyName("arch")] public Architecture Architecture { get; set; } = Architecture.Unknown;

    /// <summary>
    /// The specific OS compatible with this library. If this is <see cref="string.Empty"/>, it
    /// is compatible with any OS with the given architecture. 
    /// </summary>
    [JsonPropertyName("os")] public string OperatingSystemIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The installed version of the library. Subsequent updates increment this number.
    /// </summary>
    [JsonPropertyName("version")] public int Version { get; set; } = 0;

    /// <summary>
    /// The absolute path to the library files. 
    /// </summary>
    [JsonIgnore]
    public PathObject Path { get; set; }

    [JsonPropertyName("path")]
    public string PathStr {
        get => Path.ToString();
        set {
            if (value.StartsWith('/') || value.StartsWith('\\')) {
                value = value[1..];
            }
            Path = value.ToDirectoryPath();
        }
    }
}
