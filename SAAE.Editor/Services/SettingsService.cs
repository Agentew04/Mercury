using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SAAE.Editor.Models;

namespace SAAE.Editor.Services;

public class SettingsService {
    
    public string AppDirectory { get; init; }
    public string ConfigPath { get; init; }

    /// <summary>
    /// The current user settings
    /// </summary>
    public UserPreferences Preferences { get; set; }

    public SettingsService() {
        AppDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".saae");
        ConfigPath = Path.Combine(AppDirectory, "config.json");
    }

    public async Task SaveSettings() {
        await File.WriteAllTextAsync(ConfigPath, JsonSerializer.Serialize(Preferences, new JsonSerializerOptions() {
            WriteIndented = true
        }));
    }

    public async Task LoadSettings() {
        Preferences = JsonSerializer.Deserialize<UserPreferences>(await File.ReadAllTextAsync(ConfigPath))
            ?? GetDefaultPreferences();
    }
    
    /// <summary>
    /// Returns the default settings and preferences for a fresh install.
    /// </summary>
    public UserPreferences GetDefaultPreferences() => new UserPreferences() {
        CompilerPath = Path.Combine(AppDirectory, "compiler")
    };
    
}