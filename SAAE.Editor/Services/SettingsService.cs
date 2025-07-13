using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Converters;
using SAAE.Editor.Models;

namespace SAAE.Editor.Services;

public sealed class SettingsService : BaseService<SettingsService>, IDisposable {
    
    /// <summary>
    /// The directory where the application stores its configuration files
    /// and is the default location for the compiler and stdlib.
    /// </summary>
    public string AppDirectory { get; init; }
    
    /// <summary>
    /// The path to the config file. It is a file named 'config.json' that
    /// lives inside <see cref="AppDirectory"/>.
    /// </summary>
    public string ConfigPath { get; init; }

    /// <summary>
    /// The current user settings
    /// </summary>
    public UserPreferences Preferences { get; set; }

    public SettingsService() {
        Preferences = null!;
        AppDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".saae");
        ConfigPath = Path.Combine(AppDirectory, "config.json");
    }

    public async Task SaveSettings() {
        await File.WriteAllTextAsync(ConfigPath, JsonSerializer.Serialize(Preferences, SettingsSerializerContext.Default.UserPreferences));
    }

    public async Task LoadSettings() {
        Preferences = JsonSerializer.Deserialize(await File.ReadAllTextAsync(ConfigPath), SettingsSerializerContext.Default.UserPreferences)
            ?? GetDefaultPreferences();
        
        if(UpdatePreferences(Preferences)) {
            await SaveSettings();
        }
    }
    
    /// <summary>
    /// Returns the default settings and preferences for a fresh installation.
    /// </summary>
    public UserPreferences GetDefaultPreferences() => new(){
        CompilerPath = Path.Combine(AppDirectory, "compiler"),
        StdLibPath = Path.Combine(AppDirectory, "stdlib"),
        Language = CultureInfo.CurrentCulture,
        RecentProjects = []
    };

    private bool UpdatePreferences(UserPreferences preferences) {
        if(preferences.ConfigVersion == UserPreferences.LatestConfigVersion) {
            return false;
        }
        
        if (preferences.ConfigVersion == 1) {
            preferences.ConfigVersion = 2;
            preferences.Language = CultureInfo.CurrentCulture;
            Logger.LogInformation("Atualizada a versão de configuração para 2");
        }

        if (preferences.ConfigVersion == 2) {
            preferences.ConfigVersion = 3;
            preferences.RecentProjects = [];
            Logger.LogInformation("Atualizada a versao de configuracao para 3");   
        }

        if (preferences.ConfigVersion == 3) {
            preferences.ConfigVersion = 4;
            preferences.StdLibPath = Path.Combine(AppDirectory, "stdlib");
            Logger.LogInformation("Atualizada a versao de configuracao para 4");
        }
        // ir adicionando novos updates aqui abaixo
        return true;
    }
    
    public void Dispose() {
        // ATENCAO: nao dah pra usar async aqui por algum motivo obscuro.
        // faz escrita blocking
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(Preferences, SettingsSerializerContext.Default.UserPreferences));
        Logger.LogInformation("Saved User Settings");
    }
}

[JsonSerializable(typeof(UserPreferences))]
[JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(CultureJsonConverter)])]
internal partial class SettingsSerializerContext : JsonSerializerContext;