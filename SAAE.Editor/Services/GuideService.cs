using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using AvaloniaEdit.Utils;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using SAAE.Editor.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Inline = Avalonia.Controls.Documents.Inline;

namespace SAAE.Editor.Services;

public sealed partial class GuideService : IDisposable {

    private bool isInitialized = false;
    
    private const string GuideNamespace = "SAAE.Editor.Assets.Localization.Guides.";
    private const string GuideExtension = ".md";
    
    public GuideService() {
        Localization.LocalizationManager.CultureChanged += LocalizeGuides;
    }

    public void Dispose() {
        Localization.LocalizationManager.CultureChanged -= LocalizeGuides;
    }
    
    private readonly List<string> guideNames = [];
    private readonly Dictionary<string, GuideChapter> chapterDictionary = [];
    private readonly Dictionary<(string name, CultureInfo culture), string> manifestDictionary = [];
    private readonly Dictionary<(string name, CultureInfo culture), GuideMetadata> localizedMetadata = [];
    private readonly Assembly assembly = typeof(GuideService).Assembly;
    
    /// <summary>
    /// Initializes the service reading guides and storing structures.
    /// </summary>
    public async Task InitializeAsync() {
        if (isInitialized) {
            return;
        }

        // manifestos completos referente aos guias
        List<string> manifests = GetGuideManifestNames();
        foreach (string manifest in manifests) {
            string rawName = manifest.Replace(GuideNamespace, "").Replace(GuideExtension, "");
            Regex cultureRemover = CultureRemoverRegex();
            string name = cultureRemover.Replace(rawName, "");
            bool repeated = guideNames.Contains(name);
            if (!repeated) {
                guideNames.Add(name);
            }
            
            var culture = new CultureInfo(cultureRemover.Match(rawName).Value.Replace(".",""));
            manifestDictionary.Add((name, culture), manifest);

            if (repeated) {
                continue;
            }
            
            GuideChapter chapter = new() {
                GuideName = name,
            };
            chapterDictionary.Add(name, chapter);
        }

        await ReadMetadataAsync();
        
        isInitialized = true;
    }

    private void LocalizeGuides(CultureInfo cultureInfo) {
        foreach ((string name, GuideChapter chapter) in chapterDictionary) {
            GuideMetadata metadata = localizedMetadata[(name, cultureInfo)];
            chapter.LocalizedTitle = metadata.Title;
        }
    }

    private async Task ReadMetadataAsync() {
        foreach (((string name, CultureInfo culture) key, string manifest) in manifestDictionary) {
            await using Stream? s = assembly.GetManifestResourceStream(manifest);
            if (s is null) {
                Console.WriteLine($"Nao consegui ler manifesto: {manifest}");
                continue;
            }

            using StreamReader sr = new(s);
            MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .Build();
            MarkdownDocument markdownDocument = Markdown.Parse(await sr.ReadToEndAsync(), pipeline);
            YamlFrontMatterBlock? yamlBlock = markdownDocument.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
            if (yamlBlock is null) {
                Console.WriteLine("Nao achei yaml front matter no markdown");
                continue;
            }
            string yaml = yamlBlock.Lines.ToString();
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var metadata = deserializer.Deserialize<GuideMetadata>(yaml);
            localizedMetadata.Add(key, metadata);
        }
    }
    
    private static List<string> GetGuideManifestNames() {
        string[] allResources = typeof(GuideService).Assembly.GetManifestResourceNames();
        List<string> guideResources = allResources
            .Where(resource => resource.EndsWith(GuideExtension))
            .Where(resource => resource.StartsWith(GuideNamespace))
            .ToList();
        return guideResources;
    }
    
    /// <summary>
    /// Returns a readonly list with all available guides
    /// </summary>
    public ReadOnlyCollection<GuideChapter> GetAvailableGuides() {
        return new ReadOnlyCollection<GuideChapter>(chapterDictionary
            .Select(x => x.Value)
            .ToList());
    }

    private string GetLocalizedGuideContent(string guideName) {
        CultureInfo culture = Localization.LocalizationManager.CurrentCulture;
        string manifest = manifestDictionary[(guideName, culture)];
        using Stream? s = assembly.GetManifestResourceStream(manifest);
        if (s is null) {
            return "";
        }

        using StreamReader sr = new(s);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// Returns a list of inlines to be used in a TextBlock
    /// that represents the guide content.
    /// </summary>
    /// <param name="guide">The guide to be processed</param>
    /// <returns>A list of inlines</returns>
    public List<Inline> BuildInlines(GuideChapter guide) {
        string guideContent = GetLocalizedGuideContent(guide.GuideName);
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .Build();
        MarkdownDocument markdownDocument = Markdown.Parse(guideContent, pipeline);
        List<Inline> inlines = [];

        foreach (Block block in markdownDocument) {
            if (block is ParagraphBlock paragraphBlock) {
                inlines.AddRange(ParseInline(paragraphBlock.Inline!));
                inlines.Add(new LineBreak()); // Adiciona uma quebra de linha entre parágrafos
            }else if (block is HeadingBlock headingBlock) {
                var headingText = new Run(GetInlineText(headingBlock.Inline!))
                {
                    FontSize = 20 - (headingBlock.Level * 2), // Ajusta o tamanho do título
                    FontWeight = headingBlock.Level == 1 ? FontWeight.Bold : FontWeight.Normal
                };
                inlines.Add(headingText);
                inlines.Add(new LineBreak());
            }
        }

        return inlines;
    }
    
    public List<Inline> BuildInlines(string guideName) {
        return BuildInlines(chapterDictionary[guideName]);
    }
    
    private static List<Inline> ParseInline(ContainerInline container)
    {
        List<Inline> result = [];

        foreach (Markdig.Syntax.Inlines.Inline inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    result.Add(new Run(literal.Content.ToString()));
                    break;

                case EmphasisInline emphasis:
                    var span = new Span();
                    foreach (Inline subInline in ParseInline(emphasis)) {
                        span.Inlines.Add(subInline);
                    }

                    span.FontStyle = emphasis.DelimiterCount == 1 ? FontStyle.Italic : FontStyle.Normal;
                    span.FontWeight = emphasis.DelimiterCount == 2 ? FontWeight.Bold : FontWeight.Normal;
                    result.Add(span);
                    break;

                case LinkInline link:
                    result.Add(new Run(link.Label));
                    break;
            }
        }

        return result;
    }

    private static string GetInlineText(ContainerInline container)
    {
        string result = "";
        foreach (var inline in container)
        {
            if (inline is LiteralInline literal)
                result += literal.Content.ToString();
        }
        return result;
    }

    [GeneratedRegex(@".\w\w-\w\w")]
    private static partial Regex CultureRemoverRegex();

    private class GuideMetadata {
        public string Title { get; set; }
    }
}

