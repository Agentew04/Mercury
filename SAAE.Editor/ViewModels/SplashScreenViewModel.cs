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
using SAAE.Editor.Localization;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using YamlDotNet.Core;
using Version = System.Version;

namespace SAAE.Editor.ViewModels;

public partial class SplashScreenViewModel : BaseViewModel {

    private const string CompilerGithubUrl = "https://github.com/Agentew04/SAAE/raw/refs/heads/clang-bin/";
    private const string StdlibVersionGithubUrl = "https://github.com/Agentew04/SAAE/raw/refs/heads/stdlib/version.json";
    private const string StdlibDownloadUrl = "https://api.github.com/repos/Agentew04/SAAE/zipball/stdlib";
    
    private readonly SettingsService settings = App.Services.GetRequiredService<SettingsService>();
    private readonly HttpClient http = App.Services.GetRequiredService<HttpClient>();

    [ObservableProperty]
    private string statusText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VersionText))]
    private Version? version = null;
    public string VersionText => $"{SplashScreenResources.VersionTextValue}: {Version?.Major ?? 0}.{Version?.Minor ?? 0}";
    
    
    public async Task InitializeAsync() {
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
        (bool hasCompiler, bool hasLinker, bool hasScript) = CheckCompiler();
        if (!hasCompiler || !hasLinker || !hasScript) {
            await DownloadTools(!hasCompiler, !hasLinker, !hasScript);
        }
        
        // inicializar guias
        StatusText = SplashScreenResources.ReadGuideTextValue;
        var guideService = App.Services.GetService<GuideService>()!;
        await guideService.InitializeAsync();
        
        // baixar/verificar stdlib
        StatusText = SplashScreenResources.CheckingStdlibValue;
        if (!await CheckStdLib()) {
            await DownloadStdlib();
        }
        

        StatusText = SplashScreenResources.DoneValue;
    }

    private void Localize(CultureInfo cultureInfo) {
        OnPropertyChanged(nameof(VersionText));
    }

    // destrutor ou idisposable?
    ~SplashScreenViewModel() {
        LocalizationManager.CultureChanged -= Localize;
    }
    
    private (bool hasCompiler, bool hasLinker, bool hasScript) CheckCompiler() {
        // TODO: check if compiler is installed e usar o do usuario se possivel. issue #1
        bool appCompiler = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "clang.exe"));
        bool appLinker = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "ld.lld.exe"));
        bool script = File.Exists(Path.Combine(settings.Preferences.CompilerPath, "linker.ld"));
        
        return (appCompiler, appLinker, script);
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

        string compilerLinkerText;
        if (getCompiler && getLinker) {
            compilerLinkerText =
                $"{SplashScreenResources.CompilerTextValue} {SplashScreenResources.ConnectorTextValue} {SplashScreenResources.LinkerTextValue}";
        }
        else if (getCompiler) {
            compilerLinkerText = SplashScreenResources.CompilerTextValue;
        }
        else {
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
            await using var fs = new FileStream(Path.Combine(settings.Preferences.CompilerPath, "clang.exe"),
                FileMode.OpenOrCreate);
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
            await using var fs = new FileStream(Path.Combine(settings.Preferences.CompilerPath, "ld.lld.exe"),
                FileMode.OpenOrCreate);
            await entryStream.CopyToAsync(fs, 81920, otherProgress);
            progress.linkerStatus = TextProgress.Status.Done;
        });
        Task scriptTask = Task.Run(async () => {
            if (!getScript) {
                return;
            }
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

    private async Task<bool> CheckStdLib() {
        // ver se ela esta presente no sistema
        string libpath = settings.Preferences.StdLibPath;
        if (!Directory.Exists(libpath)) {
            return false;
        }

        if (!File.Exists(Path.Combine(libpath, "version.json"))) {
            return false;
        }
        
        string localVersionJson = await File.ReadAllTextAsync(Path.Combine(libpath, "version.json"));
        JsonNode rootNode = JsonNode.Parse(localVersionJson)!.AsObject();
        Version localVersion = Version.Parse(rootNode["version"]?.GetValue<string>() ?? "0.0");
        DateTime lastCheck = rootNode["lastCheck"]?.GetValue<DateTime>() ?? DateTime.MinValue;
        
        if(lastCheck > DateTime.Now.AddDays(-1)) {
            // ja verifiquei a versao hoje
            return true;
        }
        
        // verificar se esta atualizada com a remota
        string versionJson;
        try {
            versionJson = await http.GetStringAsync(StdlibVersionGithubUrl);
        }
        catch (HttpRequestException e) {
            Console.WriteLine("Sem conexao. Nao foi possivel verificar a versao da stdlib. Erro: " + e.Message);
            return true;
        }
        JsonDocument versionDocument = JsonDocument.Parse(versionJson);
        JsonElement versionElement = versionDocument.RootElement;
        Version remoteVersion = Version.Parse(versionElement.GetProperty("version").GetString() ?? "0.0");
        
        // atualiza data de ultima verificação
        rootNode["lastCheck"] = DateTime.Now;
        await File.WriteAllTextAsync(Path.Combine(libpath, "version.json"), rootNode.ToJsonString());
        
        return localVersion >= remoteVersion;
    }

    private async Task DownloadStdlib() {
        // houve algum problema, vai ter que baixar toda stdlib
        // so texto, n deve ser tao grande
        StatusText = SplashScreenResources.DownloadingStdlibValue;

        HttpResponseMessage response;
        try {
            response = await http.GetAsync(StdlibDownloadUrl);
        }
        catch (HttpRequestException e) {
            Console.WriteLine("Sem internet. Nao foi possivel acessar o github. Erro: " + e.Message);
            return;
        }
        
        if (!response.IsSuccessStatusCode) {
            Console.WriteLine("Requisicao falhou. Status: " + response.StatusCode);
            return;
        }
        
        // se tem arquivos antigos da stdlib, deleta
        if (Directory.Exists(settings.Preferences.StdLibPath)) {
            Directory.Delete(settings.Preferences.StdLibPath, true);
        }
        Directory.CreateDirectory(settings.Preferences.StdLibPath);
        
        // agora extrai o zip
        
        using MemoryStream ms = new();
        using Stream download = await response.Content.ReadAsStreamAsync(default);
        await download.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);
        using ZipArchive archive = new(ms, ZipArchiveMode.Read);
        foreach (ZipArchiveEntry zipArchiveEntry in archive.Entries) {
            // remove primeira pasta
            string name = zipArchiveEntry.FullName;
            string[]? parts = name.Split('/');
            parts = parts.Skip(1).ToArray();
            name = string.Join('/', parts);
            
            // eh pasta, cria uma
            if (zipArchiveEntry.FullName.EndsWith('/')) {
                Directory.CreateDirectory(settings.Preferences.StdLibPath + "/" + name);
                continue;
            }
            
            // eh arquivo, extrai arquivo
            
            string path = Path.Combine(settings.Preferences.StdLibPath, name);
            
            // cria o diretorio se nao existir
            string? directory = Path.GetDirectoryName(path);
            if (directory is not null && !Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }
            
            // extrai o arquivo
            await using Stream entryStream = zipArchiveEntry.Open();
            await using var fs = new FileStream(path, FileMode.OpenOrCreate);
            await entryStream.CopyToAsync(fs);
        }    
    }
    
    private class TextProgress : IProgress<long> {

        public enum Status {
            Unused,
            Downloading,
            Extracting,
            Done
        }
        
        public SplashScreenViewModel vm = null!;
        public long max = 0;
        public Status compilerStatus = Status.Unused;
        public Status linkerStatus = Status.Unused;

        private int smoothingValues = 30;
        private List<double> values = [];
        
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