using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Mercury.Editor.Converters;
using Mercury.Editor.Extensions;
using Mercury.Editor.Models;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace Mercury.Editor.Services;

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
    public string TemplateSettingsPath { get; }

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

    /// <summary>
    /// The current information of stored project templates.
    /// </summary>
    public TemplateSettings TemplateSettings { get; set; } = null!;

    public SettingsService() {
        Preferences = null!;
        AppDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".mercury");
        ResourcesDirectory = AppDirectory.ToDirectoryPath().Folder("resources");
        PreferencesPath = Path.Combine(AppDirectory, "config.json");
        StdLibSettingsPath = Path.Combine(AppDirectory, "stdlib.json");
        GuideSettingsPath = Path.Combine(AppDirectory, "guide.json");
        TemplateSettingsPath = Path.Combine(AppDirectory, "templates.json");
    }

    public async Task SaveSettings() {
        // parallel serialization
        await using Stream sPref = File.OpenWrite(PreferencesPath);
        Task prefTask = JsonSerializer.SerializeAsync(sPref, Preferences, SettingsSerializerContext.Default.UserPreferences);
        sPref.SetLength(sPref.Position);
        await using Stream sStd = File.OpenWrite(StdLibSettingsPath);
        Task stdTask = JsonSerializer.SerializeAsync(sStd, StdLibSettings, SettingsSerializerContext.Default.StandardLibrarySettings);
        sStd.SetLength(sStd.Position);
        await using Stream sGuide = File.OpenWrite(GuideSettingsPath);
        Task guideTask = JsonSerializer.SerializeAsync(sGuide, GuideSettings, SettingsSerializerContext.Default.GuideSettings);
        sGuide.SetLength(sGuide.Position);
        await using Stream sTemplate = File.OpenWrite(TemplateSettingsPath);
        Task templateTask = JsonSerializer.SerializeAsync(sTemplate, TemplateSettings, SettingsSerializerContext.Default.TemplateSettings);
        sTemplate.SetLength(sTemplate.Position);

        await Task.WhenAll(prefTask, stdTask, guideTask, templateTask);
    }

    public async Task LoadSettings() {
        // parallel async loading
        Task<UserPreferences> prefTask = Deserialize(PreferencesPath, SettingsSerializerContext.Default.UserPreferences,
            GetDefaultPreferences);
        Task<StandardLibrarySettings> stdlibTask = Deserialize(StdLibSettingsPath,
            SettingsSerializerContext.Default.StandardLibrarySettings, () => new StandardLibrarySettings());
        Task<GuideSettings> guideTask = Deserialize(GuideSettingsPath, SettingsSerializerContext.Default.GuideSettings,
            () => new GuideSettings());
        Task<TemplateSettings> templateTask = Deserialize(TemplateSettingsPath,
            SettingsSerializerContext.Default.TemplateSettings, () => new TemplateSettings());

        await Task.WhenAll(prefTask, stdlibTask, guideTask, templateTask);
        Preferences = prefTask.Result;
        StdLibSettings = stdlibTask.Result;
        GuideSettings = guideTask.Result;
        TemplateSettings = templateTask.Result;

        if(UpdatePreferences(Preferences)) {
            await SaveSettings();
        }

        return;

        async Task<T> Deserialize<T>(string path, JsonTypeInfo<T> info, Func<T> factory) {
            if (!File.Exists(path)) return factory();
            await using Stream stream = File.OpenRead(path);
            try {
                return await JsonSerializer.DeserializeAsync(stream, info)
                       ?? factory();
            }
            catch (JsonException) {
                return factory();
            }
        }
    }

    /// <summary>
    /// Returns the default settings and preferences for a fresh installation.
    /// </summary>
    public UserPreferences GetDefaultPreferences() => new(){
        CompilerPath = Path.Combine(AppDirectory, "compiler"),
        Language = CultureInfo.InstalledUICulture,
        OnlineCheckFrequency = TimeSpan.FromSeconds(1), // TODO: mudar isso antes da producao
        LastOnlineCheck = DateTime.MinValue,
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
[JsonSerializable(typeof(TemplateSettings))]
[JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(CultureJsonConverter)])]
internal partial class SettingsSerializerContext : JsonSerializerContext;