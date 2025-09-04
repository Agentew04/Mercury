using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.Messaging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Localization;
using SAAE.Engine;

namespace SAAE.Editor.Models;

/// <summary>
/// Class that contains information about all installed project templates.
/// </summary>
public class TemplateSettings {

    /// <summary>
    /// Returns a list with all available templates.
    /// </summary>
    [JsonPropertyName("templates")]
    public List<Template> Templates { get; set; } = [];
}

/// <summary>
/// Represents an installed template on the system.
/// </summary>
public sealed class Template : IDisposable{

    public Template() {
        WeakReferenceMessenger.Default.Register<Template,LocalizationChangedMessage>(this, OnLocalizationChange);
        if (LocalizedNames.TryGetValue(LocalizationManager.CurrentCulture.ToString(), out string? name)) {
            nameSub = new BehaviorSubject<string>(LocalizedNames[name]);
        }
        else {
            Console.WriteLine("Could not get localization for a template");
            nameSub = new BehaviorSubject<string>("localization error. "+LocalizationManager.CurrentCulture.ToString());
        }
        Name = nameSub;
    }

    private static void OnLocalizationChange(Template recipient, LocalizationChangedMessage msg) {
        if (recipient.LocalizedNames.TryGetValue(LocalizationManager.CurrentCulture.ToString(), out string? name)) {
            recipient.nameSub?.OnNext(name);
        }
    }

    private Template(IObservable<string> nameObserver) {
        Name = nameObserver;
    }

    /// <summary>
    /// A dictionary with the localized name of the template. The key
    /// is the culture key on the format "xx-YY".
    /// </summary>
    [JsonPropertyName("name")]
    public Dictionary<string, string> LocalizedNames { get; set; } = [];

    private readonly BehaviorSubject<string>? nameSub;
    
    [JsonIgnore]
    public IObservable<string> Name { get; }
    
    /// <summary>
    /// The path to the project file of the template.
    /// </summary>
    [JsonIgnore]
    public PathObject ProjectPath { get; set; }

    [JsonPropertyName("project")]
    public string ProjectPathStr {
        get => ProjectPath.ToString();
        set  {
            if (value.StartsWith('/') || value.StartsWith('\\')) {
                value = value[1..];
            }
            ProjectPath = value.ToFilePath();
        }
    }

    public static readonly Template Blank = new(ProjectResources.NoTemplateTextObservable) {
        ProjectPathStr = "",
        Version = 0,
        Architecture = Architecture.Unknown,
        LocalizedNames = [],
    };

    [JsonIgnore]
    public bool IsBlank => ReferenceEquals(this,Blank);
    
    /// <summary>
    /// The linear version number of the template.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }
    
    /// <summary>
    /// The architecture of this template. May be used to filter available
    /// templates.
    /// </summary>
    [JsonPropertyName("arch")]
    public Architecture Architecture { get; set; }

    /// <summary>
    /// The unique non-localizable identifier of this template.
    /// </summary>
    [JsonPropertyName("id")]
    public string Identifier { get; set; } = string.Empty;
    
    /// <summary>
    /// The operating system identifier that this template uses.
    /// </summary>
    [JsonPropertyName("os")] public string OperatingSystemIdentifier { get; set; } = string.Empty;

    public void Dispose() {
        nameSub?.Dispose();
        WeakReferenceMessenger.Default.Unregister<LocalizationChangedMessage>(this);
    }
}