using System.Collections.Generic;
using System.Text.Json.Serialization;
using SAAE.Editor.Extensions;
using SAAE.Engine;
using SAAE.Engine.Common;

namespace SAAE.Editor.Models;

public class GuideSettings {

    /// <summary>
    /// The current version of the guides that is downloaded.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }
    
    /// <summary>
    /// The path to the guides that are included in all projects 
    /// </summary>
    [JsonIgnore]
    public PathObject Common { get; set; }

    [JsonPropertyName("common")]
    public string CommonStr {
        get => Common.ToString();
        set {
            if (value.StartsWith('/') || value.StartsWith('\\')) {
                value = value[1..];
            }
            Common = value.ToDirectoryPath();
        }
    }

    /// <summary>
    /// A list with all architecture specific guides.
    /// </summary>
    [JsonPropertyName("architectures")]
    public List<GuideArchitecture> Architectures { get; set; } = [];

}

public class GuideArchitecture {
    
    /// <summary>
    /// The specific architecture of this guide collection.
    /// </summary>
    [JsonPropertyName("arch")]
    public Architecture Architecture { get; set; }
    
    /// <summary>
    /// The path to this collection.
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
    
    /// <summary>
    /// A list with all OS-specific guides for this architecture. 
    /// </summary>
    [JsonPropertyName("os")]
    public List<GuideOs> Os { get; set; } = [];
}

public class GuideOs {
    
    /// <summary>
    /// The name identifier for the target OS of this collection.
    /// </summary>
    [JsonPropertyName("name")] 
    public string Identifier { get; set; } = string.Empty;
    
    /// <summary>
    /// The path to the guides in this collection.
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
