namespace SAAE.Editor.Models;

public class UserPreferences {
    public int ConfigVersion { get; set; } = 1;
    public string CompilerPath { get; set; }
}