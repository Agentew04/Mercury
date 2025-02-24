using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SAAE.Editor.Converters;
using SAAE.Editor.Models;

namespace SAAE.Editor.Services;

public sealed class SettingsService : IDisposable {
    
    public string AppDirectory { get; init; }
    public string ConfigPath { get; init; }

    /// <summary>
    /// The current user settings
    /// </summary>
    public UserPreferences Preferences { get; set; }

    private readonly JsonSerializerOptions jsonOptions;

    public SettingsService() {
        Preferences = null!;
        AppDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".saae");
        ConfigPath = Path.Combine(AppDirectory, "config.json");
        jsonOptions = new JsonSerializerOptions {
            WriteIndented = true,
            Converters = { new CultureJsonConverter() }
        };
    }

    public async Task SaveSettings() {
        await File.WriteAllTextAsync(ConfigPath, JsonSerializer.Serialize(Preferences, jsonOptions));
    }

    public async Task LoadSettings() {
        Preferences = JsonSerializer.Deserialize<UserPreferences>(await File.ReadAllTextAsync(ConfigPath), jsonOptions)
            ?? GetDefaultPreferences();
        
        if(Preferences.ConfigVersion < UserPreferences.LatestConfigVersion) {
            UpdatePreferences(Preferences);
            await SaveSettings();
        }
    }
    
    /// <summary>
    /// Returns the default settings and preferences for a fresh installation.
    /// </summary>
    public UserPreferences GetDefaultPreferences() => new UserPreferences() {
        CompilerPath = Path.Combine(AppDirectory, "compiler"),
        Language = CultureInfo.CurrentCulture,
        RecentProjects = []
    };

    private static void UpdatePreferences(UserPreferences preferences) {
        if(preferences.ConfigVersion == UserPreferences.LatestConfigVersion) {
            return;
        }
        
        if (preferences.ConfigVersion == 1) {
            preferences.ConfigVersion = 2;
            preferences.Language = CultureInfo.CurrentCulture;
            Console.WriteLine("Atualizada a versão de configuração para 2");
        }

        if (preferences.ConfigVersion == 2) {
            preferences.ConfigVersion = 3;
            preferences.RecentProjects = [];
            Console.WriteLine("Atualizada a versão de configuração para 3");    
        }
        // ir adicionando novos updates aqui abaixo
    }
    
    public void Dispose() {
        // ATENCAO: nao dah pra usar async aqui por algum motivo obscuro.
        // faz escrita blocking
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(Preferences, jsonOptions));
        Console.WriteLine("Saved User Settings");
    }
}