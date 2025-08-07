using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Converters;
using SAAE.Editor.Extensions;
using SAAE.Editor.Models;
using SkiaSharp;

namespace SAAE.Editor.Services;

public sealed class SettingsService : BaseService<SettingsService>, IDisposable {
    
    /// <summary>
    /// The directory where the application stores its configuration files
    /// and is the default location for the compiler and stdlib.
    /// </summary>
    public string AppDirectory { get; }
    
    public PathObject ResourcesDirectory { get; }
    
    /// <summary>
    /// The path to the config file. It is a file named 'config.json' that
    /// lives inside <see cref="AppDirectory"/>.
    /// </summary>
    public string PreferencesPath { get; }
    
    public string StdLibSettingsPath { get; }
    
    public string GuideSettingsPath { get; }

    /// <summary>
    /// The current user settings
    /// </summary>
    public UserPreferences Preferences { get; set; }

    /// <summary>
    /// The current settings and state of the standard library.
    /// </summary>
    public StandardLibrarySettings StdLibSettings { get; set; } = null!;

    /// <summary>
    /// The current settings for the guides installed.
    /// </summary>
    public GuideSettings GuideSettings { get; set; } = null!;

    public SettingsService() {
        Preferences = null!;
        AppDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".saae");
        ResourcesDirectory = AppDirectory.ToDirectoryPath().Folder("resources");
        PreferencesPath = Path.Combine(AppDirectory, "config.json");
        StdLibSettingsPath = Path.Combine(AppDirectory, "stdlib.json");
        GuideSettingsPath = Path.Combine(AppDirectory, "guide.json");
    }

    public async Task SaveSettings() {
        await using Stream sPref = File.OpenWrite(PreferencesPath);
        await JsonSerializer.SerializeAsync(sPref, Preferences, SettingsSerializerContext.Default.UserPreferences);
        sPref.SetLength(sPref.Position);
        await using Stream sStd = File.OpenWrite(StdLibSettingsPath);
        await JsonSerializer.SerializeAsync(sStd, StdLibSettings, SettingsSerializerContext.Default.StandardLibrarySettings);
        sStd.SetLength(sStd.Position);
        await using Stream sGuide = File.OpenWrite(GuideSettingsPath);
        await JsonSerializer.SerializeAsync(sGuide, GuideSettings, SettingsSerializerContext.Default.GuideSettings);
        sGuide.SetLength(sGuide.Position);
    }

    public async Task LoadSettings() {
        if (File.Exists(PreferencesPath)) {
            await using Stream sPref = File.OpenRead(PreferencesPath);
            try {
                Preferences =
                    await JsonSerializer.DeserializeAsync(sPref, SettingsSerializerContext.Default.UserPreferences)
                    ?? GetDefaultPreferences();
            }
            catch (JsonException) {
                Preferences = GetDefaultPreferences();
            }
        }
        else {
            Preferences = GetDefaultPreferences();
        }

        if (File.Exists(StdLibSettingsPath)) {
            await using Stream sStd = File.OpenRead(StdLibSettingsPath);
            try {
                StdLibSettings =
                    await JsonSerializer.DeserializeAsync(sStd,
                        SettingsSerializerContext.Default.StandardLibrarySettings)
                    ?? new StandardLibrarySettings();
            }
            catch (JsonException) {
                StdLibSettings = new StandardLibrarySettings();
            }
        }
        else {
            StdLibSettings = new StandardLibrarySettings();
        }

        if (File.Exists(GuideSettingsPath)) {
            await using Stream sGuide = File.OpenRead(GuideSettingsPath);
            try {
                GuideSettings =
                    await JsonSerializer.DeserializeAsync(sGuide, SettingsSerializerContext.Default.GuideSettings)
                    ?? new GuideSettings();
            }
            catch (JsonException) {
                GuideSettings = new GuideSettings();
            }
        }
        else {
            GuideSettings = new GuideSettings();
        }

        if(UpdatePreferences(Preferences)) {
            await SaveSettings();
        }
    }

    /// <summary>
    /// Returns the default settings and preferences for a fresh installation.
    /// </summary>
    public UserPreferences GetDefaultPreferences() => new(){
        CompilerPath = Path.Combine(AppDirectory, "compiler"),
        Language = CultureInfo.CurrentCulture,
        OnlineCheckFrequency = TimeSpan.FromSeconds(1), // TODO: mudar isso antes da producao
        LastOnlineCheck = DateTime.MinValue
    };

    private bool UpdatePreferences(UserPreferences preferences) {
        if(preferences.ConfigVersion == UserPreferences.LatestConfigVersion) {
            return false;
        }
        // TODO: talvez mudar isso para manipular JSON diretamente?
        // ir adicionando novos updates aqui abaixo
        return true;
    }
    
    public void Dispose() {
        // ATENCAO: nao dah pra usar async aqui por algum motivo obscuro.
        // faz escrita blocking
        File.WriteAllText(PreferencesPath, JsonSerializer.Serialize(Preferences, SettingsSerializerContext.Default.UserPreferences));
    }
}

[JsonSerializable(typeof(UserPreferences))]
[JsonSerializable(typeof(StandardLibrarySettings))]
[JsonSerializable(typeof(GuideSettings))]
[JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(CultureJsonConverter)])]
internal partial class SettingsSerializerContext : JsonSerializerContext;