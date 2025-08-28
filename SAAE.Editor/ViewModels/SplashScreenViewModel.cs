using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using SAAE.Editor.Extensions;
using SAAE.Editor.Localization;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using SAAE.Editor.Views;
using Version = System.Version;

namespace SAAE.Editor.ViewModels;

public sealed partial class SplashScreenViewModel : BaseViewModel<SplashScreenViewModel, SplashScreen>, IDisposable {

    private const string CompilerGithubUrl = "https://github.com/Agentew04/SAAE/raw/refs/heads/clang-bin/";
    private const string ResourcesStructureUrl =
        "https://github.com/Agentew04/SAAE/raw/refs/heads/stdlib/structure.json";
    private const string ResourcesDownloadUrl = "https://api.github.com/repos/Agentew04/SAAE/zipball/stdlib";
    
    private readonly SettingsService settings = App.Services.GetRequiredService<SettingsService>();
    private readonly HttpClient http = App.Services.GetRequiredService<HttpClient>();
    private readonly UpdaterService updater = App.Services.GetRequiredService<UpdaterService>();

    [ObservableProperty]
    private string statusText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VersionText))]
    private Version? version;
    public string VersionText => $"{Version?.Major ?? 0}.{Version?.Minor ?? 0}";

    private TaskCompletionSource? downloadResourcesTask;
    
    public async Task InitializeAsync() {
        Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0,0);
        LocalizationManager.CultureChanged += Localize;

        Directory.CreateDirectory(settings.AppDirectory);
        
        await settings.LoadSettings();
        if(!File.Exists(settings.PreferencesPath) || string.IsNullOrEmpty(await File.ReadAllTextAsync(settings.PreferencesPath))) {
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

        UpdateInstaller(); // removes old installer on post update
        if (doOnlineCheck) {
            await UpdateSoftware();
        }
        
        List<Task> tasks = [
            DownloadCompiler(),
            DownloadGuides(doOnlineCheck),
            DownloadStdlib(doOnlineCheck),
            //DownloadTemplates(doOnlineCheck)
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

    private async Task DownloadTools(bool getAssembler, bool getLinker, bool getScript) {
        // get structure of remote repo
        StatusText = SplashScreenResources.PlatformCheckValue;
        string repoStructureJson = await http.GetStringAsync(CompilerGithubUrl + "structure.json");
        using JsonDocument repoStructure = JsonDocument.Parse(repoStructureJson);
        string os = OperatingSystem.IsWindows() ? "windows" : OperatingSystem.IsMacOS() ? "mac" : "linux";
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
            IMsBox<ButtonResult>? msgBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams() {
                ShowInCenter = true,
                CanResize = false,
                ContentMessage = $"Platform currently not supported: ({os}/{arch})",
                ContentHeader = "Not Supported",
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "Not Supported",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = true,
                EnterDefaultButton = ClickEnum.Ok,
                EscDefaultButton = ClickEnum.Ok
            });
            SplashScreen? view = GetView();
            if (view is not null) {
                await msgBox.ShowWindowDialogAsync(GetView());
            }
            return;
        }

        bool available = info.GetProperty("available").GetBoolean();
        if (!available) {
            // plataforma nao disponivel ainda
            // disparar message box
            IMsBox<ButtonResult>? msgBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams() {
                ShowInCenter = true,
                CanResize = false,
                ContentMessage = $"Platform currently not supported: ({os}/{arch})",
                ContentHeader = "Not Supported",
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "Not Supported",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = true,
                EnterDefaultButton = ClickEnum.Ok,
                EscDefaultButton = ClickEnum.Ok
            });
            SplashScreen? view = GetView();
            if (view is not null) {
                await msgBox.ShowWindowDialogAsync(GetView());
            }
            return;
        }

        // download
        string? assemblerPath = info.GetProperty("mc").GetString();
        string? linkerPath = info.GetProperty("lld").GetString();
        string? scriptPath = info.GetProperty("linkerscript").GetString();

        if (assemblerPath is null || linkerPath is null || scriptPath is null) {
            // eh o fim. :(
            // nao tem caminho
            // disparar message box
            Logger.LogError("The remote tools json doesn't have the 'mc', 'lld' or 'linkerscript' property! Can't download resources!");
            return;
        }

        if (assemblerPath.StartsWith('/')) {
            assemblerPath = assemblerPath[1..];
        }

        if (linkerPath.StartsWith('/')) {
            linkerPath = linkerPath[1..];
        }

        if (scriptPath.StartsWith('/')) {
            scriptPath = scriptPath[1..];
        }

        assemblerPath = CompilerGithubUrl + assemblerPath;
        linkerPath = CompilerGithubUrl + linkerPath;
        scriptPath = CompilerGithubUrl + scriptPath;

        if (!Directory.Exists(settings.Preferences.CompilerPath)) {
            Directory.CreateDirectory(settings.Preferences.CompilerPath);
        }

        Task assemblerTask = Task.Run(async () => {
            if (!getAssembler) {
                return;
            }
            
            StatusText = SplashScreenResources.DownloadingResourcesTextValue;

            using MemoryStream zipStream = new();
            using HttpRequestMessage requestMessage = new(HttpMethod.Get, assemblerPath);
            using HttpResponseMessage response =
                await http.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("Failed to download assembler. Error code: {err} ({reason})", response.StatusCode,
                    response.ReasonPhrase);
                return;
            }
            
            Logger.LogInformation("Downloading assembler from {compilerPath}", assemblerPath);
            Logger.LogInformation("Assembler download size: {size}", response.Content.Headers.ContentLength);
            await using Stream contentStream = await response.Content.ReadAsStreamAsync();
            await contentStream.CopyToAsync(zipStream);
            Logger.LogInformation("Download complete");
            

            zipStream.Seek(0, SeekOrigin.Begin);
            using ZipArchive archive = new(zipStream, ZipArchiveMode.Read);
            ZipArchiveEntry? entry = archive.GetEntry("llvm-mc.exe");
            if (entry is null) {
                return;
            }

            await using Stream entryStream = entry.Open();
            await using var fs = new FileStream(Path.Combine(settings.Preferences.CompilerPath, "llvm-mc.exe"),
                FileMode.OpenOrCreate);
            await entryStream.CopyToAsync(fs);
        });
        Task linkerTask = Task.Run(async () => {
            if (!getLinker) {
                return;
            }
            StatusText = SplashScreenResources.DownloadingResourcesTextValue;
            
            using MemoryStream zipStream = new();
            using HttpRequestMessage requestMessage = new(HttpMethod.Get, linkerPath);
            using HttpResponseMessage response =
                await http.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("Failed to download linker. Error code: {err} ({reason})", response.StatusCode,
                    response.ReasonPhrase);
                return;
            }
            
            Logger.LogInformation("Downloading linker from {linkerPath}. Size: {size}", linkerPath, response.Content.Headers.ContentLength);
            await using Stream contentStream = await response.Content.ReadAsStreamAsync();
            await contentStream.CopyToAsync(zipStream);
            Logger.LogInformation("Linker download completed, extracting...");

            zipStream.Seek(0, SeekOrigin.Begin);
            using ZipArchive archive = new(zipStream, ZipArchiveMode.Read);
            ZipArchiveEntry? entry = archive.GetEntry("ld.lld.exe");
            if (entry is null) {
                return;
            }

            await using Stream entryStream = entry.Open();
            await using var fs = new FileStream(Path.Combine(settings.Preferences.CompilerPath, "ld.lld.exe"),
                FileMode.OpenOrCreate);
            //progress?.AddNewQuota((ulong)entryStream.Length);
            await entryStream.CopyToAsync(fs);
        });
        Task scriptTask = Task.Run(async () => {
            if (!getScript) {
                return;
            }
            StatusText = SplashScreenResources.DownloadingResourcesTextValue;
            using HttpRequestMessage request = new(HttpMethod.Get, scriptPath);
            using HttpResponseMessage response =
                await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("Failed to download script. Error code: {err}", response.StatusCode);
                return;
            }
                
            Logger.LogInformation("Downloading linker script from {scriptPath}", scriptPath);
            await using Stream download = await response.Content.ReadAsStreamAsync();
            await using var fs = new FileStream(Path.Combine(settings.Preferences.CompilerPath, "linker.ld"),
                FileMode.OpenOrCreate);
            await download.CopyToAsync(fs);
            Logger.LogInformation("Linker script downloaded successfully");
        });  

        await Task.WhenAll(assemblerTask, linkerTask, scriptTask);
        StatusText = SplashScreenResources.DoneDownloadingValue;
    }

    private Task DownloadCompiler() {
        bool hasAssembler = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "llvm-mc.exe"));
        bool hasLinker = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "ld.lld.exe"));
        bool hasLinkerScript = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "linker.ld"));

        if (hasAssembler && hasLinker && hasLinkerScript) {
            return Task.CompletedTask;
        }

        return DownloadTools(!hasAssembler, !hasLinker, !hasLinkerScript);
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

        await RequestDownload();
        
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

        await RequestDownload();
        
        // atualiza settings com as novas versoes
        settings.StdLibSettings.AvailableLibraries = libs.ForEachExt(x => {
            x.Path = settings.ResourcesDirectory + x.Path;
        }).ToList();
    }

    private async Task DownloadTemplates(bool doOnlineCheck) {
        if (!doOnlineCheck) {
            return;
        }
        
        string json = await http.GetStringAsync(ResourcesStructureUrl);
        using JsonDocument structureDoc = JsonDocument.Parse(json);
        JsonElement templatesProperty = structureDoc.RootElement.GetProperty("templates");
        
        // se o remote tem mais templates que nos
        bool doDownload = templatesProperty.GetArrayLength() > settings.TemplateSettings.Templates.Count;
        List<Template> remoteTemplates = [];
        if (!doDownload) {
            // ou algum dos nossos templates esta desatualzado
            using JsonElement.ArrayEnumerator arrayEnumerator = templatesProperty.EnumerateArray();
            foreach (JsonElement templateElement in arrayEnumerator) {
                int version = templateElement.GetProperty("version").GetInt32();
                string id = templateElement.GetProperty("id").GetString() ?? string.Empty;
                Template? localTemplate = settings.TemplateSettings.Templates.FirstOrDefault(x => x.Identifier == id);
                if (localTemplate is null) {
                    doDownload = true;
                    break;
                }

                if (localTemplate.Version < version) {
                    doDownload = true;
                    break;
                }
            }

            if (doDownload) {
                arrayEnumerator.Reset();
                remoteTemplates = arrayEnumerator
                    .Select(x => x.Deserialize(SettingsSerializerContext.Default.Template))
                    .Where(x => x is not null)
                    .ToList()!;
            }
        }

        if (!doDownload) {
            return;
        }
        
        await RequestDownload();
        
        // atualiza settings dos templates
        settings.TemplateSettings.Templates.ForEach(x => x.Dispose());
        settings.TemplateSettings.Templates.Clear();
        settings.TemplateSettings.Templates.AddRange(remoteTemplates.ForEachExt(x => {
                x.ProjectPath = settings.ResourcesDirectory + x.ProjectPath;
            }
        ));
    }
    
    private Task RequestDownload()
    {
        if (downloadResourcesTask is not null)
        {
            return downloadResourcesTask.Task;
        }
        
        downloadResourcesTask = new TaskCompletionSource();
        DownloadResources().ContinueWith(t =>
        {
            if (t.Result)
            {
                downloadResourcesTask!.SetResult();
            }
            else
            {
                downloadResourcesTask!.SetCanceled();
            }
        });
        return downloadResourcesTask.Task;
    }

    private async Task<bool> DownloadResources()
    {
        StatusText = SplashScreenResources.DownloadingResourcesTextValue;


        Logger.LogInformation("Downloading new resources from {url}", ResourcesDownloadUrl);
        using HttpRequestMessage request = new(HttpMethod.Get, ResourcesDownloadUrl);
        using HttpResponseMessage response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Logger.LogError("Failed to fetch new resources. Error code: {err} ({reason})", response.StatusCode,
                response.ReasonPhrase);
            return false;
        }


        // before extracting, delete old resources
        if (Directory.Exists(settings.ResourcesDirectory.ToString()))
        {
            Directory.Delete(settings.ResourcesDirectory.ToString(), true);
        }

        using MemoryStream zipStream = new();
        await using Stream contentStream = await response.Content.ReadAsStreamAsync();
        await contentStream.CopyToAsync(zipStream);
        zipStream.Seek(0, SeekOrigin.Begin);
        try
        {
            using ZipArchive zip = new(zipStream, ZipArchiveMode.Read);
            Logger.LogInformation("Extracting resources from zip archive");
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                string name = entry.FullName;
                string[] parts = name.Split('/',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                parts = parts.Skip(1).ToArray();
                // eh pasta, cria uma
                if (entry.FullName.EndsWith('/'))
                {
                    Directory.CreateDirectory(settings.ResourcesDirectory.Folders(parts).ToString());
                    Logger.LogInformation("Creating Folder {folder}",
                        settings.ResourcesDirectory.Folders(parts).ToString());
                    continue;
                }

                // eh arquivo, extrai arquivo
                PathObject filePath = settings.ResourcesDirectory.Folders(parts[..^1]).File(parts[^1]);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath.ToString())!);

                // extrai o arquivo
                Logger.LogInformation("Extracting file {file}", filePath.ToString());
                try
                {
                    await using Stream entryStream = entry.Open();
                    await using FileStream fs = new(filePath.ToString(), FileMode.OpenOrCreate);
                    await entryStream.CopyToAsync(fs);
                }
                catch (Exception e)
                {
                    Logger.LogError("Cannot extract entry {entry.FullName} from zip archive: {error}.", entry.FullName,
                        e.Message);
                }
            }

            Logger.LogInformation("Resources downloaded and extracted successfully");
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError("Error downloading and extracting zip. Error: {error}. StackTrace: {stack}", e.Message, e.StackTrace);
            return false;
        }

    }

    private void UpdateInstaller() {
        PathObject appLocation = Assembly.GetAssembly(typeof(App))!.Location.ToFilePath()
            .Path();
        PathObject newInstaller = appLocation.File("Updater2.exe");
        if (!newInstaller.Exists()) return;
        Logger.LogInformation("Removing old updater and using new one");
        PathObject oldInstaller = appLocation.File("Updater.exe");
        oldInstaller.Delete();
        File.Delete(oldInstaller.ToString());
        File.Move(newInstaller.ToString(), oldInstaller.ToString());
    }
    
    private async Task UpdateSoftware() {
        
        GithubRelease? latest = (await updater.GetRemoteReleases())
            .Where(x => x.Version > Version)
            .OrderByDescending(x => x.Version)
            .FirstOrDefault();
        if (latest is null) {
            return;
        }
        IMsBox<ButtonResult>? messageBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams() {
            ShowInCenter = true,
            CloseOnClickAway = false,
            EnterDefaultButton = ClickEnum.Yes,
            EscDefaultButton = ClickEnum.No,
            CanResize = false,
            ContentTitle = SplashScreenResources.UpdatePromptTitleValue,
            ContentMessage = SplashScreenResources.UpdatePromptBodyValue,
            Topmost = true,
            ButtonDefinitions = ButtonEnum.YesNo,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        });
        ButtonResult result;
        SplashScreen? view = GetView();
        if (view is not null) {
             result = await messageBox.ShowWindowDialogAsync(view);
        }
        else {
            result = await messageBox.ShowWindowAsync();
        }

        if (result != ButtonResult.Yes) {
            return;
        }

        GithubAsset asset = latest.Assets.First(); // talvez uma selecao mais rigorosa aqui
        using MemoryStream ms = new();
        Logger.LogInformation("Downloading release {version} from {date}", latest.Version, latest.PublishDate.Date.ToShortDateString());
        StatusText = SplashScreenResources.DownloadingUpdateTextValue;
        await updater.DownloadAsset(asset, ms);
        Logger.LogInformation("Unpacking release");
        StatusText = SplashScreenResources.UnpackingUpdateTextValue;
    
        string artifactFolder = await updater.UnpackAsset(asset, ms);
        Logger.LogInformation("Updating. Bye-Bye");
        updater.Update(artifactFolder);
    }

}