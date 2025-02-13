using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Localization;
using SAAE.Editor.Models;
using SAAE.Editor.Services;

namespace SAAE.Editor.ViewModels;

public partial class SplashScreenViewModel : BaseViewModel {

    private const string GithubUrl = "https://github.com/Agentew04/SAAE/raw/refs/heads/clang-bin/";
    private readonly SettingsService settings = App.Services.GetService<SettingsService>()!;

    [ObservableProperty]
    private string statusText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VersionText))]
    private Version? version = null;
    public string VersionText => $"{SplashScreenResources.VersionTextValue}: {Version?.Major ?? 0}.{Version?.Minor ?? 0}";
    
    
    public async Task Initialize() {
        Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0,0);
        LocalizationManager.CultureChanged += Localize;
        
        if(!Directory.Exists(settings.AppDirectory) || !File.Exists(settings.ConfigPath)) {
            Directory.CreateDirectory(settings.AppDirectory);
            
            // write default configuration
            StatusText = SplashScreenResources.StdSettingsDefineValue;
            settings.Preferences = settings.GetDefaultPreferences();
            await settings.SaveSettings();
        }
        
        // read stored configuration
        await settings.LoadSettings();
        
        LocalizationManager.CurrentCulture = settings.Preferences.Language;
        
        // baixar compilador
        StatusText = SplashScreenResources.ToolchainCheckValue;
        (bool hasCompiler, bool hasLinker) = CheckCompiler();
        if (!hasCompiler || !hasLinker) {
            await DownloadCompiler(!hasCompiler, !hasLinker);
        }
        
        // inicializar guias
        StatusText = SplashScreenResources.ReadGuideTextValue;
        var guideService = App.Services.GetService<GuideService>()!;
        await guideService.InitializeAsync();

        StatusText = SplashScreenResources.DoneValue;
        await Task.Delay(1000);
    }

    private void Localize(CultureInfo cultureInfo) {
        OnPropertyChanged(nameof(VersionText));
    }

    ~SplashScreenViewModel() {
        LocalizationManager.CultureChanged -= Localize;
    }
    
    
    private (bool hasCompiler, bool hasLinker) CheckCompiler() {
        // TODO: check if compiler is installed e usar o do usuario se possivel
        bool appCompiler = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "clang.exe"));
        bool appLinker = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "ld.lld.exe"));
        return (appCompiler, appLinker);
    }
    
    private async Task DownloadCompiler(bool getCompiler, bool getLinker) {
        // get structure of remote repo
        StatusText = SplashScreenResources.PlatformCheckValue;
        using HttpClient http = new();
        string repoStructureJson = await http.GetStringAsync(GithubUrl + "structure.json");
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

        if (compilerPath is null || linkerPath is null) {
            // eh o fim. :(
            // nao tem caminho
            // disparar message box
            return;
        }

        if (compilerPath.StartsWith('/')) {
            compilerPath = compilerPath[1..];
        }
        if (linkerPath.StartsWith('/')) {
            linkerPath = linkerPath[1..];
        }
        compilerPath = GithubUrl + compilerPath;
        linkerPath = GithubUrl + linkerPath;
        
        if (!Directory.Exists(settings.Preferences.CompilerPath)) {
            Directory.CreateDirectory(settings.Preferences.CompilerPath);
        }
        
        string compilerLinkerText;
        if (getCompiler && getLinker) {
            compilerLinkerText =
                $"{SplashScreenResources.CompilerTextValue} {SplashScreenResources.ConnectorTextValue} {SplashScreenResources.LinkerTextValue}";
        }else if (getCompiler) {
            compilerLinkerText = SplashScreenResources.CompilerTextValue;
        }else {
            compilerLinkerText = SplashScreenResources.LinkerTextValue;
        }

        StatusText = SplashScreenResources.DownloadingTextValue + compilerLinkerText;
                     
        TextProgress progress = new() { vm = this };
        Task compilerTask = Task.Run(async () => {
            if (!getCompiler) {
                return;
            }
            progress.compilerStatus = TextProgress.Status.Downloading;
            MemoryStream ms = new();
            using HttpResponseMessage response =
                await http.GetAsync(compilerPath, HttpCompletionOption.ResponseHeadersRead);
            long? contentLength = response.Content.Headers.ContentLength;
            progress.max += contentLength ?? 0;

            await using Stream download = await response.Content.ReadAsStreamAsync(default);
            Progress<long> otherProgress = new(progress.Report);
            if (contentLength.HasValue) {
                // posso reportar
                await download.CopyToAsync(ms, 81920, otherProgress);
                progress.Report(1);
            }
            else {
                // n sei o tamanho total, so faz o download
                await download.CopyToAsync(ms);
            }

            progress.compilerStatus = TextProgress.Status.Extracting;
            ms.Seek(0, SeekOrigin.Begin);
            using ZipArchive archive = new(ms, ZipArchiveMode.Read);
            ZipArchiveEntry? entry = archive.GetEntry("clang.exe");
            if (entry is null) {
                return;
            }
            await using Stream entryStream = entry.Open();
            progress.max += entry.Length;
            await using var fs = new FileStream(Path.Combine(settings.Preferences.CompilerPath, "clang.exe"),FileMode.OpenOrCreate);
            await entryStream.CopyToAsync(fs, 81920, otherProgress);
            progress.compilerStatus = TextProgress.Status.Done;
        });
        Task linkerTask = Task.Run(async () => {
            if (!getLinker) {
                return;
            }
            
            progress.linkerStatus = TextProgress.Status.Downloading;
            using MemoryStream ms = new();
            using HttpResponseMessage response =
                await http.GetAsync(linkerPath, HttpCompletionOption.ResponseHeadersRead);
            long? contentLength = response.Content.Headers.ContentLength;
            progress.max += contentLength ?? 0;

            await using Stream download = await response.Content.ReadAsStreamAsync(default);
            Progress<long> otherProgress = new(progress.Report);
            if (contentLength.HasValue) {
                // posso reportar
                await download.CopyToAsync(ms, 81920, otherProgress);
                progress.Report(1);
            }
            else {
                // n sei o tamanho total, so faz o download
                await download.CopyToAsync(ms);
            }

            progress.linkerStatus = TextProgress.Status.Extracting;
            ms.Seek(0, SeekOrigin.Begin);
            using ZipArchive archive = new(ms, ZipArchiveMode.Read);
            ZipArchiveEntry? entry = archive.GetEntry("ld.lld.exe");
            if (entry is null) {
                return;
            }
            progress.max += entry.Length;
            await using Stream entryStream = entry.Open();
            await using var fs = new FileStream(Path.Combine(settings.Preferences.CompilerPath, "ld.lld.exe"),FileMode.OpenOrCreate);
            await entryStream.CopyToAsync(fs, 81920, otherProgress);
            progress.linkerStatus = TextProgress.Status.Done;
        });

        await Task.WhenAll(compilerTask, linkerTask);
        StatusText = SplashScreenResources.DoneDownloadingValue;
    }

    private class TextProgress : IProgress<long> {

        public enum Status {
            Unused,
            Downloading,
            Extracting,
            Done
        }
        
        public SplashScreenViewModel vm;
        public long max = 0;
        public Status compilerStatus = Status.Unused;
        public Status linkerStatus = Status.Unused;

        private int smoothingValues = 30;
        private List<double> values = new();
        
        public void Report(long value) {
            double relative = (double)value / (double)max;
            if (values.Count < smoothingValues) {
                values.Add(relative);
            }
            else {
                values.RemoveAt(0);
                values.Add(relative);
            }
            
            double smoothed = values.Average();
            string percentage = smoothed.ToString("P");
            
            if(compilerStatus > Status.Unused && linkerStatus > Status.Unused) {
                // os dois
                string mainText = ((compilerStatus >= Status.Extracting && compilerStatus != Status.Done) 
                                   || (linkerStatus >= Status.Extracting && linkerStatus != Status.Done)) ?
                    SplashScreenResources.ExtractingTextValue : SplashScreenResources.DownloadingTextValue;
                vm.StatusText = $"{mainText} {SplashScreenResources.CompilerTextValue} {SplashScreenResources.ConnectorTextValue} {SplashScreenResources.LinkerTextValue} ({percentage})";
            }else if (compilerStatus > Status.Unused) {
                // soh compiler
                vm.StatusText = $"{(compilerStatus >= Status.Extracting && compilerStatus != Status.Done ?
                    SplashScreenResources.ExtractingTextValue : SplashScreenResources.DownloadingTextValue)} {SplashScreenResources.CompilerTextValue} ({percentage})"; 
            }
            else {
                // soh linker
                vm.StatusText = $"{(linkerStatus >= Status.Extracting && linkerStatus != Status.Done ?
                    SplashScreenResources.ExtractingTextValue : SplashScreenResources.DownloadingTextValue)} {SplashScreenResources.LinkerTextValue} ({percentage})";
            }
        }
    }
    
    
}