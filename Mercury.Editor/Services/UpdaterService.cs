using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mercury.Editor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers;

namespace Mercury.Editor.Services;

public partial class UpdaterService : BaseService<UpdaterService> {

    private readonly Uri releasesUri = new("https://api.github.com/repos/Agentew04/Mercury/releases");
    private readonly HttpClient http = App.Services.GetRequiredService<HttpClient>();

    public async Task<List<GithubRelease>> GetRemoteReleases(CancellationToken cancellationToken = default) {
        try {
            using HttpRequestMessage request = new(HttpMethod.Get, releasesUri);
            using HttpResponseMessage response = await http.SendAsync(request, cancellationToken);
            await using Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using JsonDocument document =
                await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken);
            using JsonElement.ArrayEnumerator releasesEnumerator = document.RootElement.EnumerateArray();
            List<GithubRelease> releases = [];
            foreach (JsonElement releaseElement in releasesEnumerator) {
                GithubRelease? release = ParseRelease(releaseElement);
                if (release is not null) {
                    releases.Add(release);
                }
            }

            return releases;
        }
        catch (Exception ex) {
            Logger.LogError("Could not fetch remote version information. Error Message: {msg}", ex.Message);
            return [];
        }
    }

    private GithubRelease? ParseRelease(JsonElement element) {
        string tag = element.GetProperty("tag_name").GetString() ?? string.Empty;
        string name = element.GetProperty("name").GetString() ?? string.Empty;
        string[] candidates = [
            tag,
            name
        ];
        Regex reg3 = Version3NumberRegex();
        Regex reg2 = Version2NumberRegex();
        bool found = false;
        Version? version = null;
        for (int i = 0; i < candidates.Length && !found; i++) {
            string subject = candidates[i];
            Match match = reg3.Match(subject);
            if (match.Groups.Count < 2) {
                // try with X.Y style version
                match = reg2.Match(subject);
                if (match.Groups.Count < 2) continue;
            }

            try {
                version = Version.Parse(match.Groups[1].ValueSpan);
            }
            catch (Exception) {
                // nada                
            }
            found = true;
        }

        if (version is null) {
            return null;
        }
        
        DateTime publishDate = element.GetProperty("published_at").GetDateTime();
        using JsonElement.ArrayEnumerator assetEnumerator = element.GetProperty("assets").EnumerateArray();
        List<GithubAsset> assets = [];
        foreach (JsonElement assetElement in assetEnumerator) {
            GithubAsset asset = ParseAsset(assetElement);
            assets.Add(asset);
        }

        return new GithubRelease() {
            Assets = assets,
            Version = version,
            PublishDate = publishDate
        };
    }

    [GeneratedRegex(@".*(\d+\.\d+\.\d+).*")]
    private partial Regex Version3NumberRegex();
    
    [GeneratedRegex(@".*(\d+\.\d+).*")]
    private partial Regex Version2NumberRegex();


    private GithubAsset ParseAsset(JsonElement element) {
        string name = element.GetProperty("name").GetString() ?? string.Empty;
        Uri url = new(element.GetProperty("browser_download_url").GetString() ?? string.Empty);
        DateTime uploadDate = element.GetProperty("updated_at").GetDateTime();
        ulong size = element.GetProperty("size").GetUInt64();
        GithubFileType type = name.Split('.')[^1] switch {
            "rar" => GithubFileType.Rar,
            "zip" => GithubFileType.Zip,
            "gz" => GithubFileType.TarGz, // <- grok is this true?
            "exe" => GithubFileType.Exe,
            "dll" => GithubFileType.Dll,
            _ => GithubFileType.Unknown
        };
            
        return new GithubAsset {
            Name = name,
            DownloadUrl = url,
            UploadDate = uploadDate,
            Size = size,
            Type = type
        };
    }

    public async Task DownloadAsset(GithubAsset asset, Stream outputStream,
        CancellationToken cancellationToken = default) {
        using HttpRequestMessage request = new(HttpMethod.Get, asset.DownloadUrl);
        using HttpResponseMessage response = await http.SendAsync(request, cancellationToken);
        await using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await contentStream.CopyToAsync(outputStream, cancellationToken);
    }

    public async Task<string> UnpackAsset(GithubAsset asset, Stream assetStream) {
        assetStream.Seek(0, SeekOrigin.Begin);
        switch (asset.Type) {
            // simple file case
            case GithubFileType.Exe:
            case GithubFileType.Dll:
            case GithubFileType.Unknown: {
                Logger.LogWarning("Tried unpacking unknown file type. Assuming plain file.");
                string path = Path.GetTempFileName();
                await using FileStream fs = File.OpenWrite(path);
                await assetStream.CopyToAsync(fs);
                return path;
            }
            case GithubFileType.Zip: {
                Logger.LogInformation("Extracting .zip archive");
                using ZipArchive archive = new(assetStream, ZipArchiveMode.Update, leaveOpen: true);
                string path = Path.Combine(Path.GetTempPath(), Random.Shared.Next().ToString());
                Directory.CreateDirectory(path);
                archive.ExtractToDirectory(path, true);
                return path;
            }
            case GithubFileType.Rar: {
                Logger.LogError("Extracting .rar archive");
                using RarArchive archive = RarArchive.Open(assetStream, new ReaderOptions {
                    LeaveStreamOpen = true
                });
                string path = Path.Combine(Path.GetTempPath(), Random.Shared.Next().ToString());
                Directory.CreateDirectory(path);
                archive.ExtractToDirectory(path);
                return path;
            }
            case GithubFileType.TarGz: {
                Logger.LogInformation("Extracting .tar.gz archive");
                await using GZipStream gz = new(assetStream, CompressionMode.Decompress, leaveOpen: true);
                await using TarReader reader = new(gz);
                string path = Path.Combine(Path.GetTempPath(), Random.Shared.Next().ToString());
                Directory.CreateDirectory(path);
                TarEntry? entry;
                while ((entry = await reader.GetNextEntryAsync()) != null) {
                    if (entry.EntryType == TarEntryType.Directory) {
                        Logger.LogInformation("Creating directory {path}", Path.Combine(path, entry.Name));
                        Directory.CreateDirectory(Path.Combine(path, entry.Name));
                    }else if (entry.EntryType is TarEntryType.RegularFile
                              or TarEntryType.ContiguousFile
                              or TarEntryType.SparseFile
                              or TarEntryType.V7RegularFile) {
                        Logger.LogInformation("Extracting file entry: {name}", entry.Name);
                        await entry.ExtractToFileAsync(Path.Combine(path, entry.Name), true);
                    }
                    else {
                        Logger.LogWarning("Unknown tar entry. Type: {type}", entry.EntryType);
                    }
                }

                return path;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(asset), "Invalid Asset Type");
        }
    }

    public void Update(string newPackageDirectory) {
        string[] args = Environment.GetCommandLineArgs();
        if (args[0].EndsWith(".dll")) {
            args[0] = args[0].Replace(".dll", ".exe"); // :)
        }
        int pid = Environment.ProcessId;
        PathObject appLocation = Assembly.GetAssembly(typeof(App))!.Location.ToFilePath()
            .Path();

        if (!File.Exists(appLocation.File("Updater.exe").ToString())) {
            Logger.LogError("Could not find Updater executable! Will not update");
            return;
        }
        
        ProcessStartInfo startInfo = new() {
            FileName = appLocation.File("Updater.exe").ToString(),
            Arguments = $"{pid} {appLocation} {newPackageDirectory} {string.Join(" ",args)}"
        };
        Process.Start(startInfo);
        App.Shutdown();
    }
}


/// <summary>
/// Class that represents a release published in github. 
/// </summary>
public class GithubRelease {
    
    /// <summary>
    /// The version of this release. Must be parsed from "tag_name", "name" and [zip/ball]balls urls.
    /// </summary>
    public required Version Version { get; init; }
    
    /// <summary>
    /// A list with all uploaded assets linked to this release.
    /// </summary>
    public required List<GithubAsset> Assets { get; init; }
    
    /// <summary>
    /// The date that this release was published.
    /// </summary>
    public required DateTime PublishDate { get; init; }
}

/// <summary>
/// Class to hold relevant information about a uploaded asset of a <see cref="GithubRelease"/>.
/// </summary>
public class GithubAsset {
    /// <summary>
    /// The url to directly download the asset.
    /// </summary>
    public required Uri DownloadUrl { get; init; }
    
    /// <summary>
    /// The name of the asset.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Date when this asset was uploaded.
    /// </summary>
    public required DateTime UploadDate { get; init; }
    
    /// <summary>
    /// The absolute size of the asset in bytes.
    /// </summary>
    public required ulong Size { get; init; }
    
    /// <summary>
    /// The recognized filetype that this asset is. Used to know which
    /// processing method to use to unpack.
    /// </summary>
    public required GithubFileType Type { get; set; }
}

public enum GithubFileType {
    Unknown,
    Zip,
    Rar,
    TarGz,
    Exe,
    Dll
}