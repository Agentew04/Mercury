using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Mercury.Editor.Extensions;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.Services;

/// <summary>
/// Service responsible for parsing theme files on startup and
/// registering them to the app's ResourceDictionary as theme
/// variants.
/// </summary>
/// <remarks>
/// Automatically includes a XML schema on theme folder. This schema is not
/// used by the parser. Only there to help anyone that wants to extend the app.
/// </remarks>
public class ThemeService : BaseService<ThemeService> {

    private readonly SettingsService settingsService;
    private readonly ILogger<ThemeService> logger;
    private readonly List<ThemeVariant> variants = [];

    public ThemeService(SettingsService settings, ILogger<ThemeService> logger) {
        settingsService = settings;
        this.logger = logger;
    }
    
    /// <summary>
    /// Loads all available themes from the disk and adds them to the
    /// <see cref="ResourceDictionary.ThemeDictionaries"/> of the passed dictionary.
    /// </summary>
    /// <param name="dict">The <see cref="ResourceDictionary"/> that will receive the variants.</param>
    public void LoadThemes(IResourceDictionary dict) {
        string dir = settingsService.ThemesDirectory.ToString();

        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
            // no files exist. add schema for anyone that wants to add a new file
            File.WriteAllText(Path.Combine(dir,"schema.xsd"), ThemeSchema);
        }

        // dir exists, create schema if it does not exist
        if (!File.Exists(Path.Combine(dir, "schema.xsd"))) {
            File.WriteAllText(Path.Combine(dir,"schema.xsd"), ThemeSchema);
        }
        
        PopulateDefaultThemes();
        
        List<Theme> themes = [];
        foreach (string file in Directory.EnumerateFiles(dir)) {
            if (Path.GetExtension(file) != ".xml") {
                continue;
            }
            Theme? theme;
            try {
                theme = ReadTheme(file);
            }
            catch (XmlException ex) {
                logger.LogWarning("Could not parse theme. File: {File}; Error: {Error}", file, ex.Message);
                continue;
            }
            themes.Add(theme);
        }
        
        // all themes read. check for duplicate names
        IEnumerable<string> duplicates = themes.GroupBy(x => x.Name)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key);
        if (duplicates.Any()) {
            logger.LogWarning("Found two or more themes with the same name.");
        }

        IEnumerable<Theme> deduplicated = themes.DistinctBy(x => x.Name);
        foreach (Theme theme in deduplicated) {
            ThemeVariant variant = theme.Name switch {
                "Light" or "light" => ThemeVariant.Light,
                "Dark" or "dark" => ThemeVariant.Dark,
                _ => new ThemeVariant(theme.Name, theme.Inherits == ThemeInheritance.Light ? ThemeVariant.Light : ThemeVariant.Dark)
            };
            variants.Add(variant);
            ResourceDictionary themeDictionary = CreateDictionary(theme);
            logger.LogInformation("Registered theme: {theme}", variant.Key);
            dict.ThemeDictionaries.Add(variant, themeDictionary);
        }
    }
    
    /// <summary>
    /// Returns a list with all registered theme variants.
    /// </summary>
    public IReadOnlyList<ThemeVariant> GetAvailableThemes() {
        return variants;
    }

    public void SetApplicationTheme(ThemeVariant theme) {
        if (Application.Current is not null) {
            logger.LogInformation("Setting RequestedThemeVariant: {Variant}", theme.Key);
            Application.Current.RequestedThemeVariant = theme;
        }
    }
    
    private Theme ReadTheme(string file) {
        using FileStream fs = File.OpenRead(file);
        Theme theme = new();
        
        XmlReader reader = XmlReader.Create(fs, new XmlReaderSettings() {
            IgnoreComments = true,
            IgnoreWhitespace = true
        });
        
        reader.MoveToContent();
        if (reader.NodeType != XmlNodeType.Element || reader.Name != "Theme") {
            throw new XmlException("Root element must be <Theme>");
        }
        
        theme.Name = reader.GetAttribute("name") 
                     ?? throw new XmlException("Theme missing required attribute 'name'");
        
        string? inheritsAttr = reader.GetAttribute("inherits");
        if (inheritsAttr != null)
        {
            theme.Inherits = inheritsAttr switch
            {
                "Light" or "light" => ThemeInheritance.Light,
                "Dark" or "dark" => ThemeInheritance.Dark,
                _ => throw new XmlException($"Invalid inherits value '{inheritsAttr}'")
            };
        }
        else {
            throw new XmlException("Missing inherits value for theme.");
        }
        
        // Entra no elemento
        if (reader.IsEmptyElement) {
            return theme;
        }

        reader.Read(); // entra no conteúdo

        // Loop pelos filhos
        while (reader.ReadState == ReadState.Interactive) {
            if (reader is { NodeType: XmlNodeType.EndElement, Name: "Theme" }) {
                break;
            }

            if (reader is { NodeType: XmlNodeType.Element, Name: "Color" }) {
                var c = new Color {
                    Name  = reader.GetAttribute("name") 
                            ?? throw new XmlException("Color missing 'name' attribute"),
                    Value = reader.GetAttribute("value") 
                            ?? throw new XmlException("Color missing 'value' attribute")
                };

                theme.Colors.Add(c);
            }

            reader.Read();
        }

        return theme;
        
    }

    private ResourceDictionary CreateDictionary(Theme theme) {
        ResourceDictionary dict = new();
        foreach (Color colorElement in theme.Colors) {
            Avalonia.Media.Color color = ParseColor(colorElement.Value);
            
            // add brush
            dict.Add(colorElement.Name, new SolidColorBrush(color));
            
            // add color
            dict.Add(colorElement.Name + "Color", color);
        }

        return dict;
    }

    private Avalonia.Media.Color ParseColor(string str) {
        if (string.IsNullOrWhiteSpace(str) || str[0] != '#') {
            throw new Exception("Invalid color format");
        }

        // #RRGGBB
        if (str.Length == 7) {
            byte r = Convert.ToByte(str.Substring(1, 2), 16);
            byte g = Convert.ToByte(str.Substring(3, 2), 16);
            byte b = Convert.ToByte(str.Substring(5, 2), 16);

            return new Avalonia.Media.Color(255, r, g, b);
        }
        logger.LogError("Could not parse input \"{input}\" as a color. Defaulting to magenta.", str);
        return new Avalonia.Media.Color(255, 255, 0, 255);
    }

    private void PopulateDefaultThemes() {
        PathObject dir = settingsService.ThemesDirectory;

        PathObject dark = dir.File("Dark.xml"); 
        PathObject light = dir.File("Light.xml");
        
        if (!dark.Exists()) {
            using Stream? s = typeof(ThemeService).Assembly.GetManifestResourceStream("Mercury.Editor.Assets.Themes.Dark.xml");
            if (s is null) {
                logger.LogWarning("Could not find resource name for default dark theme");
            } else {
                using FileStream fs = File.OpenWrite(dark.ToString());
                s.CopyTo(fs);
                logger.LogInformation("Creating default Dark theme file: {file}", dark);
                fs.SetLength(fs.Position);
            }
        }
        if (!light.Exists()) {
            using Stream? s = typeof(ThemeService).Assembly.GetManifestResourceStream("Mercury.Editor.Assets.Themes.Light.xml");
            if (s is null) {
                logger.LogWarning("Could not find resource name for default light theme");
            } else {
                using FileStream fs = File.OpenWrite(light.ToString());
                s.CopyTo(fs);
                logger.LogInformation("Creating default Light theme file: {file}", light);
                fs.SetLength(fs.Position);
            }
        }
    }
    
    private sealed class Theme
    {
        public string Name { get; set; } = "";
        public List<Color> Colors { get; } = [];
        public ThemeInheritance Inherits { get; set; } = ThemeInheritance.None;
    }

    private sealed class Color
    {
        public string Name { get; init; } = "";
        public string Value { get; init; } = "";
    }

    private const string ThemeSchema =
        """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="https://rodrigoappelt.com/mercury/theme"
                   xmlns="https://rodrigoappelt.com/mercury/theme"
                   elementFormDefault="qualified">

          <!-- cor valor -->
          <xs:simpleType name="hexColorType">
            <xs:restriction base="xs:string">
              <xs:pattern value="#[A-Fa-f0-9]{6}"/>
            </xs:restriction>
          </xs:simpleType>
          <!-- enum heranca -->
          <xs:simpleType name="inheritMode">
            <xs:restriction base="xs:string">
              <xs:enumeration value="Light"/>
              <xs:enumeration value="Dark"/>
            </xs:restriction>
          </xs:simpleType>
          <!-- cor elemento -->
          <xs:element name="Color">
            <xs:complexType>
              <xs:attribute name="name" type="xs:string" use="required"/>
              <xs:attribute name="value" type="hexColorType" use="required"/>
            </xs:complexType>
          </xs:element>
          <!-- elemento tema -->
          <xs:element name="Theme">
            <xs:complexType>
              <xs:sequence>
                <xs:element ref="Color" minOccurs="0" maxOccurs="unbounded"/>
              </xs:sequence>
              <xs:attribute name="inherits" type="inheritMode" use="required"/>
              <xs:attribute name="name" type="xs:string" use="required"/>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;
    
    private enum ThemeInheritance
    {
        None,
        Light,
        Dark
    }
}