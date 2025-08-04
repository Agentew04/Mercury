using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Localization;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using YamlDotNet.Core;
using Version = System.Version;

namespace SAAE.Editor.ViewModels;

public sealed partial class SplashScreenViewModel : BaseViewModel<SplashScreenViewModel>, IDisposable {

    private const string CompilerGithubUrl = "https://github.com/Agentew04/SAAE/raw/refs/heads/clang-bin/";
    private const string StdlibVersionGithubUrl = "https://github.com/Agentew04/SAAE/raw/refs/heads/stdlib/version.json";

    private const string ResourcesStructureUrl =
        "https://github.com/Agentew04/SAAE/raw/refs/heads/stdlib/structure.json";
    private const string ResourcesDownloadUrl = "https://api.github.com/repos/Agentew04/SAAE/zipball/stdlib";
    
    private readonly SettingsService settings = App.Services.GetRequiredService<SettingsService>();
    private readonly HttpClient http = App.Services.GetRequiredService<HttpClient>();

    [ObservableProperty]
    private string statusText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VersionText))]
    private Version? version = null;
    public string VersionText => $"{SplashScreenResources.VersionTextValue}: {Version?.Major ?? 0}.{Version?.Minor ?? 0}";

    private TaskCompletionSource? downloadResourcesTask;
    
    
    public async Task InitializeAsync() {
        Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0,0);
        LocalizationManager.CultureChanged += Localize;

        Directory.CreateDirectory(settings.AppDirectory);
        
        await settings.LoadSettings();
        if(!File.Exists(settings.PreferencesPath)
           || string.IsNullOrEmpty(await File.ReadAllTextAsync(settings.PreferencesPath))) {
            // write default configuration
            StatusText = SplashScreenResources.StdSettingsDefineValue;
            settings.Preferences = settings.GetDefaultPreferences();
            await settings.SaveSettings();
        }

        if (!File.Exists(settings.StdLibSettingsPath) || !File.Exists(settings.GuideSettingsPath)
            || string.IsNullOrEmpty(await File.ReadAllTextAsync(settings.StdLibSettingsPath))
            || string.IsNullOrEmpty(await File.ReadAllTextAsync(settings.GuideSettingsPath))) {
            settings.StdLibSettings = new StandardLibrarySettings();
            settings.GuideSettings = new GuideSettings();
            await settings.SaveSettings();
        }
        
        // read stored configuration
        
        
        LocalizationManager.CurrentCulture = settings.Preferences.Language;
        
        // baixar compilador
        StatusText = SplashScreenResources.InitializingTextValue;

        bool doOnlineCheck = DateTime.Now - settings.Preferences.LastOnlineCheck > settings.Preferences.OnlineCheckFrequency;
        if (doOnlineCheck) {
            settings.Preferences.LastOnlineCheck = DateTime.Now;
        }
        await settings.SaveSettings();

        List<Task> tasks = [
            DownloadCompiler(),
            DownloadGuides(doOnlineCheck),
            DownloadStdlib(doOnlineCheck)
        ];
        

        await Task.WhenAll(tasks);
        await settings.SaveSettings();
        
        StatusText = SplashScreenResources.DoneValue;
    }

    private void Localize(CultureInfo cultureInfo) {
        OnPropertyChanged(nameof(VersionText));
    }

    public void Dispose() {
        LocalizationManager.CultureChanged -= Localize;
    }

    private async Task DownloadTools(bool getCompiler, bool getLinker, bool getScript) {
        // get structure of remote repo
        StatusText = SplashScreenResources.PlatformCheckValue;
        string repoStructureJson = await http.GetStringAsync(CompilerGithubUrl + "structure.json");
        using JsonDocument repoStructure = JsonDocument.Parse(repoStructureJson);
        string os = OperatingSystem.IsWindows() ? "windows" : "linux";
        string arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";

        // get compiler and linker path in remote repo
        JsonElement info;
        try {
            info = repoStructure.RootElement
                .GetProperty(os)
                .GetProperty(arch);
        }
        catch (KeyNotFoundException) {
            // erro, plataforma nao suportada
            // eh disparado em linux 32bits, macos, arm etc
            // disparar message box
            return;
        }

        bool available = info.GetProperty("available").GetBoolean();
        if (!available) {
            // plataforma nao disponivel ainda
            // disparar message box
            return;
        }

        // download
        string? compilerPath = info.GetProperty("clang").GetString();
        string? linkerPath = info.GetProperty("lld").GetString();
        string? scriptPath = info.GetProperty("linkerscript").GetString();

        if (compilerPath is null || linkerPath is null || scriptPath is null) {
            // eh o fim. :(
            // nao tem caminho
            // disparar message box
            Logger.LogError("The remote tools json doesn't have the 'clang', 'lld' or 'linkerscript' property! Can't download resources!");
            return;
        }

        if (compilerPath.StartsWith('/')) {
            compilerPath = compilerPath[1..];
        }

        if (linkerPath.StartsWith('/')) {
            linkerPath = linkerPath[1..];
        }

        if (scriptPath.StartsWith('/')) {
            scriptPath = scriptPath[1..];
        }

        compilerPath = CompilerGithubUrl + compilerPath;
        linkerPath = CompilerGithubUrl + linkerPath;
        scriptPath = CompilerGithubUrl + scriptPath;

        if (!Directory.Exists(settings.Preferences.CompilerPath)) {
            Directory.CreateDirectory(settings.Preferences.CompilerPath);
        }

        Task compilerTask = Task.Run(async () => {
            if (!getCompiler) {
                return;
            }
            
            StatusText = SplashScreenResources.DownloadingResourcesTextValue;

            MemoryStream ms = new();
            using HttpResponseMessage response =
                await http.GetAsync(compilerPath, HttpCompletionOption.ResponseHeadersRead);
            await response.Content.CopyToAsync(ms);

            ms.Seek(0, SeekOrigin.Begin);
            using ZipArchive archive = new(ms, ZipArchiveMode.Read);
            ZipArchiveEntry? entry = archive.GetEntry("clang.exe");
            if (entry is null) {
                return;
            }

            await using Stream entryStream = entry.Open();
            await using var fs = new FileStream(Path.Combine(settings.Preferences.CompilerPath, "clang.exe"),
                FileMode.OpenOrCreate);
            await entryStream.CopyToAsync(fs);
        });
        Task linkerTask = Task.Run(async () => {
            if (!getLinker) {
                return;
            }
            StatusText = SplashScreenResources.DownloadingResourcesTextValue;
            
            using MemoryStream ms = new();
            using HttpResponseMessage response =
                await http.GetAsync(linkerPath, HttpCompletionOption.ResponseHeadersRead);
            await response.Content.CopyToAsync(ms);

            ms.Seek(0, SeekOrigin.Begin);
            using ZipArchive archive = new(ms, ZipArchiveMode.Read);
            ZipArchiveEntry? entry = archive.GetEntry("ld.lld.exe");
            if (entry is null) {
                return;
            }

            await using Stream entryStream = entry.Open();
            await using var fs = new FileStream(Path.Combine(settings.Preferences.CompilerPath, "ld.lld.exe"),
                FileMode.OpenOrCreate);
            await entryStream.CopyToAsync(fs);
        });
        Task scriptTask = Task.Run(async () => {
            if (!getScript) {
                return;
            }
            StatusText = SplashScreenResources.DownloadingResourcesTextValue;
            using HttpResponseMessage response =
                await http.GetAsync(scriptPath, HttpCompletionOption.ResponseContentRead);
            await using Stream download = await response.Content.ReadAsStreamAsync(default);
            await using var fs = new FileStream(Path.Combine(settings.Preferences.CompilerPath, "linker.ld"),
                FileMode.OpenOrCreate);
            await download.CopyToAsync(fs);
        });  

        await Task.WhenAll(compilerTask, linkerTask, scriptTask);
        StatusText = SplashScreenResources.DoneDownloadingValue;
    }

    private Task DownloadCompiler() {
        bool hasCompiler = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "clang.exe"));
        bool hasLinker = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "ld.lld.exe"));
        bool hasLinkerScript = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "linker.ld"));

        if (hasCompiler && hasLinker && hasLinkerScript) {
            return Task.CompletedTask;
        }

        return DownloadTools(!hasCompiler, !hasLinker, !hasLinkerScript);
    }
    
    private async Task DownloadGuides(bool doOnlineCheck) {
        if (settings.GuideSettings.Version != 0 && !doOnlineCheck) {
            return;
        }

        string json = await http.GetStringAsync(ResourcesStructureUrl);
        using JsonDocument structureDoc = JsonDocument.Parse(json);
        JsonElement guideProperty = structureDoc.RootElement.GetProperty("guides");
        int remoteVersion = guideProperty.GetProperty("version").GetInt32();
        if (remoteVersion <= settings.GuideSettings.Version) {
            // ja esta atualizado
            return;
        }

        GuideSettings? guideSettings = guideProperty.Deserialize<GuideSettings>(SettingsSerializerContext.Default.GuideSettings);
        if (guideSettings is null) {
            Logger.LogError("Detected newer version for guides but couldn't parse structrue json. Consider updating the app.");
            return;
        }

        await DownloadResources();
        
        // update current guide settings with modified paths from new guide settings
        settings.GuideSettings.Version = guideSettings.Version;
        settings.GuideSettings.Common = settings.ResourcesDirectory + guideSettings.Common;
        settings.GuideSettings.Architectures = guideSettings.Architectures
            .ForEachExt(x => {
                x.Path = settings.ResourcesDirectory + x.Path;
                x.Os.ForEach(y => {
                    y.Path = settings.ResourcesDirectory + y.Path;
                });
            })
            .ToList();
        // guide initialization is execute after project selection
        // to correctly filter arch and os guides. 
    }


    private async Task DownloadStdlib(bool doOnlineCheck) {
        if (settings.StdLibSettings.AvailableLibraries.Any(x => x.Version != 0)
            && !doOnlineCheck) {
            // has at least one library installed and already checked today, skip
            return;
        }
        
        string json = await http.GetStringAsync(ResourcesStructureUrl);
        using JsonDocument structureDoc = JsonDocument.Parse(json);
        JsonElement stdlibProperty = structureDoc.RootElement.GetProperty("stdlib");
        bool download = settings.StdLibSettings.AvailableLibraries.Count < stdlibProperty.GetArrayLength();

        JsonElement.ArrayEnumerator arrayEnumerator = stdlibProperty.EnumerateArray();
        List<StandardLibrary> libs = arrayEnumerator
            .Select(x => x.Deserialize(SettingsSerializerContext.Default.StandardLibrary))
            .Where(x => x is not null)
            .ToList()!;
        arrayEnumerator.Dispose();
        foreach (StandardLibrary installedLibrary in settings.StdLibSettings.AvailableLibraries) {
            if(download)break;
            StandardLibrary? target = libs
                .Find(x => x.Architecture == installedLibrary.Architecture
                           && x.OperatingSystemIdentifier == installedLibrary.OperatingSystemIdentifier);
            if (target is null) {
                continue;
            }

            if (target.Version <= installedLibrary.Version) continue;
            download = true;
            break;
        }

        if (!download) {
            return;
        }

        await DownloadResources();
        
        // atualiza settings com as novas versoes
        settings.StdLibSettings.AvailableLibraries = libs.ForEachExt(x => {
            x.Path = settings.ResourcesDirectory + x.Path;
        }).ToList();
    }

    private async Task DownloadResources() {
        if (downloadResourcesTask is not null) {
            await downloadResourcesTask.Task;
            return;
        }

        StatusText = SplashScreenResources.DownloadingResourcesTextValue;
        
        downloadResourcesTask = new TaskCompletionSource();
        Logger.LogInformation("Downloading new resources");
        HttpResponseMessage response = await http.GetAsync(ResourcesDownloadUrl);
        if (!response.IsSuccessStatusCode) {
            Logger.LogError("Failed to fetch new resources. Error code: {err} ({reason})", response.StatusCode, response.ReasonPhrase);
            downloadResourcesTask.SetCanceled();
            return;
        }
        
        
        // before extracting, delete old resources
        Directory.Delete(settings.ResourcesDirectory.ToString(), true);

        MemoryStream ms = new();
        await response.Content.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);
        ZipArchive zip = new(ms, ZipArchiveMode.Read);
        await Parallel.ForEachAsync(zip.Entries, async (entry, c) => {
            string name = entry.FullName;
            string[] parts = name.Split('/');
            parts = parts.Skip(1).ToArray();
            // eh pasta, cria uma
            if (entry.FullName.EndsWith('/')) {
                Directory.CreateDirectory(settings.ResourcesDirectory.Folders(parts).ToString());
                Logger.LogInformation("Creating Folder {folder}", settings.ResourcesDirectory.Folders(parts).ToString());
                return;
            }

            // eh arquivo, extrai arquivo
            PathObject filePath = settings.ResourcesDirectory.Folders(parts[..^1]).File(parts[^1]);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath.ToString())!);

            // extrai o arquivo
            Logger.LogInformation("Extracting file {file}", filePath.ToString());
            await using Stream entryStream = entry.Open();
            await using var fs = new FileStream(filePath.ToString(), FileMode.OpenOrCreate);
            await entryStream.CopyToAsync(fs, c);
        });
        downloadResourcesTask.SetResult();
    }
}