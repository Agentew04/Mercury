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
        
        #nullable enable
        
        namespace SAAE.Editor.Localization;
        
        public static class LocalizationManager
        {
            private static CultureInfo currentCulture = new("pt-BR");
            public static CultureInfo CurrentCulture {
                get => currentCulture;
                set {
                    if (currentCulture != value) {
                        currentCulture = value;
                        CultureChanged?.Invoke(currentCulture);
                    }
                }
            }
            
            public static event Action<CultureInfo>? CultureChanged = null;
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
            
            spc.AddSource("SAAE.Editor.Localization.LocalizationManager.g.cs", SourceText.From(LocalizationManagerCode, Encoding.UTF8));
            
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
        foreach (LocalizationEntry entry in entries)
        {
            properties.AppendLine(
                $"""
                     /// <summary>
                     /// Property that retrieves the value of the {entry.Name} resource in
                     /// the current culture.
                     /// </summary>
                     public string {entry.Name} => resourceManager.GetString("{entry.Name}", LocalizationManager.CurrentCulture) ?? "";
                     
                     /// <summary>
                     /// Property that retrieves the value of the {entry.Name} resource in
                     /// the current culture.
                     /// </summary>
                     public static string {entry.Name}Value => Instance.{entry.Name};
                 """);

            updates.Append(
                $"""
                 
                         OnPropertyChanged(new("{entry.Name}"));
                 """);
        }
        
        string classCode = 
        $$"""
        using System;
        using System.ComponentModel;
        using System.Globalization;
        
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
            }
            
            private void OnCultureChanged(CultureInfo culture) {
                Console.WriteLine("Culture changed to " + culture.Name + ". Reloading strings...");
        {{updates}}
            }
            
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void OnPropertyChanged(string? propertyName = null) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            
        {{properties}}
        }                
        """;
        ctx.AddSource($"SAAE.Editor.Localization.{module}.g.cs", SourceText.From(classCode, Encoding.UTF8));
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