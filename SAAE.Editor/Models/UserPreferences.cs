using System.Globalization;

namespace SAAE.Editor.Models;

public class UserPreferences {
    
    public const int LatestConfigVersion = 2;
    
    public int ConfigVersion { get; set; } = 2;
    public string CompilerPath { get; set; } = "";
    public CultureInfo Language { get; set; } = new("pt-BR");
}