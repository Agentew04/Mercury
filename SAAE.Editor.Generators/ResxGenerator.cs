using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace SAAE.Generators;

[Generator]
public class ResxGenerator : IIncrementalGenerator {
    
    private const string LocalizationManagerCode =
        """
        using System;
        using System.Globalization;
        using System.Collections.Generic;
        using CommunityToolkit.Mvvm.Messaging;
        
        #nullable enable
        
        namespace SAAE.Editor.Localization;
        
        public class LocalizationChangedMessage{
            public CultureInfo Culture { get; init; }
        }
        
        public static class LocalizationManager
        {
            private static CultureInfo currentCulture = new("pt-BR");
            
            /// <summary>
            /// Gets or sets the current culture.
            /// </summary>
            public static CultureInfo CurrentCulture {
                get => currentCulture;
                set {
                    if (currentCulture != value) {
                        currentCulture = value;
                        CultureChanged?.Invoke(currentCulture);
                        WeakReferenceMessenger.Default.Send<LocalizationChangedMessage>(new(){
                            Culture = currentCulture
                        });
                    }
                }
            }
            
            /// <summary>
            /// An event that is invoked every time the culture changes.
            /// </summary>
            public static event Action<CultureInfo>? CultureChanged = null;
            
            private static List<CultureInfo> availableCultures = [ 
        {cultures}    ];
            /// <summary>
            /// Returns a list with all the available cultures.
            /// </summary>
            public static IReadOnlyList<CultureInfo> AvailableCultures => availableCultures.AsReadOnly();
        }
        """;
    
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        
        IncrementalValueProvider<ImmutableArray<AdditionalText>> resxFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".resx"))
            .Collect();
        
        context.RegisterSourceOutput(resxFiles, (spc, files) =>
        {
            IEnumerable<string> uniqueModules = files
                .Select(file => Path.GetFileNameWithoutExtension(file.Path)) // Remove .resx
                .Select<string,string>(name => Regex.Replace(name, @"\.[a-zA-Z]{2}(-[A-Za-z]{2})?$", "")) // Remove cultura
                .Distinct();

            Regex cultureRegex = new(@".+\.(?<culture>[a-zA-Z]{2}-[a-zA-Z]{2})");
            IEnumerable<string> cultures = files
                .Select(file => Path.GetFileNameWithoutExtension(file.Path))
                .Select(name => cultureRegex.Match(name).Groups["culture"].Value)
                .Select(x => string.IsNullOrWhiteSpace(x) ? "en-US" : x) // neutral assembly
                .Distinct();

            StringBuilder cultureSb = new();
            foreach (string culture in cultures) {
                cultureSb.AppendLine($"        new(\"{culture}\"),");
            }
            
            spc.AddSource("SAAE.Editor.Localization.LocalizationManager.g.cs", 
                SourceText.From(
                    LocalizationManagerCode.Replace("{cultures}", cultureSb.ToString()),
                    Encoding.UTF8));
            
            foreach (string? module in uniqueModules)
            {
                AdditionalText? resxFile = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.Path).StartsWith(module));
                if (resxFile != null)
                {
                    GenerateClassForResx(spc, resxFile, module);
                }
            }
        });
    }

    private static void GenerateClassForResx(SourceProductionContext ctx, AdditionalText file, string module) {
        XDocument document = XDocument.Parse(file.GetText()?.ToString() ?? "");
        List<LocalizationEntry> entries = document.Descendants("data")
            .Select(e => new LocalizationEntry(
                name: e.Attribute("name")?.Value ?? "",
                value: e.Element("value")?.Value ?? "",
                comment: e.Element("comment")?.Value))
            .Where(e => !string.IsNullOrEmpty(e.Name) && !string.IsNullOrEmpty(e.Value))
            .ToList();
        StringBuilder properties = new();
        StringBuilder updates = new();
        StringBuilder ctorInits = new();
        foreach (LocalizationEntry entry in entries) {
            properties.AppendLine(
                $$"""
                     /// <summary>
                     /// Reactive value holder for property <see cref="{{entry.Name}}"/>
                     /// </summary>
                     private readonly BehaviorSubject<string> _{{entry.Name}};
                     
                     /// <summary>
                     /// Returns a <see cref="IObservable{T}"/>
                     /// </summary>
                     public static IObservable<string> {{entry.Name}}Observable => Instance._{{entry.Name}};
                 
                     /// <summary>
                     /// Property that retrieves the value of the {{entry.Name}} resource in
                     /// the current culture.
                     /// </summary>
                     public string {{entry.Name}} => resourceManager.GetString("{{entry.Name}}", LocalizationManager.CurrentCulture) ?? "";

                     /// <summary>
                     /// Function that retrieves the newest value of the <see cref="{{entry.Name}}">
                     /// property in the current culture.
                     /// </summary>
                     public string Get{{entry.Name}}() => {{entry.Name}};

                     /// <summary>
                     /// Property that retrieves the value of the {{entry.Name}} resource in
                     /// the current culture.
                     /// </summary>
                     public static string {{entry.Name}}Value => Instance.{{entry.Name}};
                 """);

            updates.AppendLine(
                $"""
                         OnPropertyChanged(new("{entry.Name}"));
                         _{entry.Name}.OnNext({entry.Name});
                 """);
            ctorInits.AppendLine(
               $"""
                        _{entry.Name} = new BehaviorSubject<string>({entry.Name});
                """);
        }
        
        string moduleclassCode = 
        $$"""
        using System;
        using System.ComponentModel;
        using System.Globalization;
        using System.Reactive.Subjects;
        
        #nullable enable
        
        namespace SAAE.Editor.Localization;
        
        /// <summary>
        /// Class that holds the localization strings for the {module} module.
        /// </summary>
        public class {{module}} : INotifyPropertyChanged {
        
            private static {{module}}? _instance = null;
            /// <summary>
            /// Public static instance of the <see cref="{{module}}"/> class.
            /// It is lazy loaded.
            /// </summary>
            public static {{module}} Instance => _instance ?? (_instance = new {{module}}());
            
            private readonly System.Resources.ResourceManager resourceManager;
            
            private {{module}}() {
                LocalizationManager.CultureChanged += OnCultureChanged;
                resourceManager = new System.Resources.ResourceManager(
                    baseName: "SAAE.Editor.Assets.Localization.{{module}}", 
                    typeof({{module}}).Assembly);
        {{ctorInits}}
            }
            
            private void OnCultureChanged(CultureInfo culture) {
        {{updates}}
            }
            
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void OnPropertyChanged(string? propertyName = null) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            
        {{properties}}
        }                
        """;
        ctx.AddSource($"SAAE.Editor.Localization.{module}.g.cs", SourceText.From(moduleclassCode, Encoding.UTF8));

        StringBuilder switchCaseEntries = new();
        StringBuilder enumEntries = new();
        foreach (LocalizationEntry entry in entries) {
            switchCaseEntries.AppendLine(
               $"""
                            case {module}Enum.{entry.Name}:
                                return {module}.{entry.Name}Observable.ToBinding();
                """);
            enumEntries.AppendLine(
                $"        {entry.Name},"
            );
        }

        string extensionClassCode = 
            $$"""
              using System;
              using Avalonia;
              using Avalonia.Data;
              using Avalonia.Markup.Xaml;
              
              namespace SAAE.Editor.Localization;
              
              /// <summary>
              /// Extension class to enable easy XAML access to localization for <see cref="{{module}}"/>.
              /// </summary>
              public class {{module}}Extension {
              
                  [ConstructorArgument("resource")]
                  public {{module}}Enum Resource { get; }
                  
                  public {{module}}Extension({{module}}Enum resource) => Resource = resource;
                  
                  public IBinding ProvideValue() {
                      switch (Resource) {
              {{switchCaseEntries}}
                      }
                      return null;
                  }
                  
                  public enum {{module}}Enum {
              {{enumEntries}}
                  }
              }
              """;
        ctx.AddSource($"SAAE.Editor.Localization.{module}Extension.g.cs", SourceText.From(extensionClassCode, Encoding.UTF8));
    }
    
    private readonly record struct LocalizationEntry {
        
        public LocalizationEntry(string name, string value, string? comment)
        {
            Name = name;
            Value = value;
            Comment = comment;
        }
        
        public string Name { get; }
        public string Value { get; }
        public string? Comment { get; }
    }
}