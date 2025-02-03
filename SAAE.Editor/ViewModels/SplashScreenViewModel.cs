using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SAAE.Editor.Models;

namespace SAAE.Editor.ViewModels;

public partial class SplashScreenViewModel : BaseViewModel {

    private const string GithubUrl = "https://github.com/Agentew04/SAAE/raw/refs/heads/clang-bin/";
    private readonly string configurationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.saae";

    private readonly string userPreferencesPath;

    private UserPreferences preferences;

    public SplashScreenViewModel() {
        userPreferencesPath = Path.Combine(configurationDirectory, "config.json");
    }
    
    public async Task Initialize() {
        if(!Directory.Exists(configurationDirectory) || !File.Exists(userPreferencesPath)) {
            Directory.CreateDirectory(configurationDirectory);
            
            // write default configuration
            StatusText = "Definindo configurações padrão";
            UserPreferences defaultConfig = GetDefaultPreferences();
            await File.WriteAllTextAsync(userPreferencesPath, JsonSerializer.Serialize(defaultConfig));
        }
        
        // read stored configuration
        string configJson = await File.ReadAllTextAsync(userPreferencesPath);
        preferences = JsonSerializer.Deserialize<UserPreferences>(configJson) ?? GetDefaultPreferences();

        StatusText = "Checking for compiler...";
        // clang
        (bool hasCompiler, bool hasLinker) = CheckCompiler();
        if (!hasCompiler || !hasLinker) {
            await DownloadCompiler(!hasCompiler, !hasLinker);
        }

        StatusText = "Done!";
        await Task.Delay(1000);
        await Task.Delay(1000);
        await Task.Delay(1000);
    }

    [ObservableProperty]
    private string statusText;

    private (bool hasCompiler, bool hasLinker) CheckCompiler() {
        // TODO: check if compiler is installed e usar o do usuario se possivel
        bool appCompiler = File.Exists(Path.Combine(preferences.CompilerPath, "clang.exe"));
        bool appLinker = File.Exists(Path.Combine(preferences.CompilerPath, "ld.lld.exe"));
        return (appCompiler, appLinker);
    }
    
    private async Task DownloadCompiler(bool getCompiler, bool getLinker) {
        string token = "?token=GHSAT0AAAAAACXMFNE6YQLTDXRKUXLYEV3WZ5BFDQA";
        // get structure of remote repo
        StatusText = "Checking compiler availability for current platform";
        using HttpClient http = new();
        string repoStructureJson = await http.GetStringAsync(GithubUrl + "structure.json"+token);
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
            return;
        }
        
        bool available = info.GetProperty("available").GetBoolean();
        if (!available) {
            // plataforma nao disponivel ainda
            return;
        }
        
        // download
        string? compilerPath = info.GetProperty("clang").GetString();
        string? linkerPath = info.GetProperty("lld").GetString();

        if (compilerPath is null || linkerPath is null) {
            // eh o fim. :(
            return;
        }

        if (compilerPath.StartsWith('/')) {
            compilerPath = compilerPath[1..];
        }
        if (linkerPath.StartsWith('/')) {
            linkerPath = linkerPath[1..];
        }
        compilerPath = GithubUrl + compilerPath+token;
        linkerPath = GithubUrl + linkerPath+token;
        
        if (getCompiler) {
            StatusText = "Downloading compiler";
            await using Stream s = await http.GetStreamAsync(compilerPath);
            using ZipArchive zip = new(s);
            await using FileStream fs = File.Open(Path.Combine(preferences.CompilerPath, "clang.exe"), FileMode.OpenOrCreate);
            await zip.GetEntry("clang.exe")!.Open().CopyToAsync(fs);
        }

        if (getLinker) {
            StatusText = "Downloading linker";
            await using Stream s = await http.GetStreamAsync(linkerPath);
            using ZipArchive zip = new(s);
            await using FileStream fs = File.Open(Path.Combine(preferences.CompilerPath, "ld.lld.exe"), FileMode.OpenOrCreate);
            await zip.GetEntry("ld.lld.exe")!.Open().CopyToAsync(fs);
        }
    }
    
    private UserPreferences GetDefaultPreferences() => new UserPreferences() {
        CompilerPath = Path.Combine(configurationDirectory, "compiler")
    };
}