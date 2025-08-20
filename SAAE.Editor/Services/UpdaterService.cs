using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SAAE.Editor.Services;

public partial class UpdaterService {

    private readonly Uri releasesUri = new("https://api.github.com/repos/Agentew04/SAAE/releases");
    private readonly HttpClient http = App.Services.GetRequiredService<HttpClient>();

    public async Task<IReadOnlyList<GithubRelease>> GetRemoteReleases(CancellationToken cancellationToken = default) {
        using HttpRequestMessage request = new(HttpMethod.Get, releasesUri);
        using HttpResponseMessage response = await http.SendAsync(request, cancellationToken);
        await using Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken);
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

    private GithubRelease? ParseRelease(JsonElement element) {
        string tag = element.GetProperty("tag_name").GetString() ?? string.Empty;
        Regex reg = Version3NumberRegex();
        Match match = reg.Match(tag);
        Version version;
        if (match.Captures.Count >= 1) {
            version = Version.Parse(match.Captures[0].ValueSpan);
        }
        else {
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

    [GeneratedRegex(@".*(\d+.\d+.\d+).*")]
    private partial Regex Version3NumberRegex();


    private GithubAsset ParseAsset(JsonElement element) {
        string name = element.GetProperty("name").GetString() ?? string.Empty;
        Uri url = new(element.GetProperty("browser_download_url").GetString() ?? string.Empty);
        DateTime uploadDate = element.GetProperty("updated_at").GetDateTime();
        ulong size = element.GetProperty("size").GetUInt64();
        return new GithubAsset {
            Name = name,
            DownloadUrl = url,
            UploadDate = uploadDate,
            Size = size
        };
    }

    public async Task DownloadAsset(GithubAsset asset, Stream outputStream,
        CancellationToken cancellationToken = default) {
        using HttpRequestMessage request = new(HttpMethod.Get, asset.DownloadUrl);
        using HttpResponseMessage response = await http.SendAsync(request, cancellationToken);
        await using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await contentStream.CopyToAsync(outputStream, cancellationToken);
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
}