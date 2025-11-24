using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Services;

// untested. :o
// alguma hora aplicar, dps do dia 17 
public class ThemeService : BaseService<ThemeService> {

    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();

    private readonly List<(ThemeVariant,ResourceDictionary)> themes = [];
    
    public void LoadThemes(ResourceDictionary dict) {
        string dir = settingsService.ThemesDirectory.ToString();

        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
            // already know there are no files
            return;
        }

        foreach (string file in Directory.EnumerateFiles(dir)) {
            ResourceDictionary themeDict = ReadTheme(file);
            string name = Path.GetFileNameWithoutExtension(file);
            ThemeVariant variant = name switch {
                "Light" or "light" => ThemeVariant.Light,
                "Dark" or "dark" => ThemeVariant.Dark,
                _ => new ThemeVariant(name, ThemeVariant.Dark)
            };
            themes.Add((variant, dict));
        }

        foreach ((ThemeVariant variant, ResourceDictionary themeDict) in themes) {
            dict.ThemeDictionaries.Add(variant, themeDict);
        }
    }
    
    private ResourceDictionary ReadTheme(string file) {
        // reads a file like .ini or .properties
        // each line is a name = #rrggbb or name=#aarrggbb
        List<ColorData> colors = [];
        bool needsTutorial = false;
        FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read);
        StreamReader sr = new(fs);
        string? line = sr.ReadLine()?.Trim();
        if (!line?.StartsWith(FirstDocumentationLine) ?? true) {
            needsTutorial = true;
        }

        do {
            line ??= string.Empty;
            StringBuilder nameSb = new();
            StringBuilder colorSb = new();
            int stage = 0;
            foreach (char c in line) {
                if(c == '#') break;
                switch (stage) {
                    case 0: {
                        // reading name
                        if (c == '=') {
                            stage++;
                            break;
                        }
                        if (!char.IsWhiteSpace(c)) {
                            nameSb.Append(c);
                        }
                        break;
                    }
                    case 1: {
                        // reading color
                        if(c == '\n' || colorSb.Length == 8) {
                            stage++;
                            break;
                        }
                        if (char.IsWhiteSpace(c) || char.IsControl(c)) {
                            continue;
                        }

                        if (c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F') {
                            colorSb.Append(c);
                        }
                        break;
                    }
                }
            }

            if (stage != 2 || (colorSb.Length != 6 && colorSb.Length != 8)
                || nameSb.Length == 0) {
                line = sr.ReadLine()?.Trim();
                continue;
            }
            
            // add color
            colors.Add(new ColorData(nameSb.ToString(), colorSb.ToString()));
            line = sr.ReadLine()?.Trim();
        } while (line is not null);

        sr.Dispose();
        fs.Dispose();

        if (needsTutorial) {
            using FileStream fs2 = new(file, FileMode.Open);
            using StreamWriter sr2 = new(fs2,leaveOpen:true);
            sr2.WriteLine(DocumentationText);
            foreach (ColorData color in colors) {
                sr2.WriteLine($"{color.Name} = {color.Color}");
            }
        }

        ResourceDictionary dict = new();
        foreach (ColorData color in colors) {
            string colorName = color.Name + "Color";
            Color color2 = Color.Parse(color.Color);
            dict.Add(color.Name, new SolidColorBrush(color2));
            dict.Add(colorName, color2);
        }
        return dict;
    }

    private readonly record struct ColorData(string Name, string Color);

    private const string FirstDocumentationLine = "# Mercury Theming Tutorial";
    private const string DocumentationText = FirstDocumentationLine +
        """

        # 
        # A theme file for the Mercury IDE consists of a list
        # of key-value pairs. The keys are names and the values
        # are hex colors. Hex colors are either 6 characters long
        # (representing RRGGBB) or 8 characters long (AARRGGBB)
        # for an alpha channel. Each character must be in the 
        # range [0-9] or [A-F] or [a-f]. Each line must follow 
        # the format:
        # propertyName = color
        # Text after a '#' character is ignored until end of line.
        # Consult default themes('Light' or 'Dark') for an example
        # of which propertyNames exist.
        """;
}