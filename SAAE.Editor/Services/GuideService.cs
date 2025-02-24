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
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using SAAE.Editor.Localization;
using SAAE.Editor.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Inline = Avalonia.Controls.Documents.Inline;
using MdInline = Markdig.Syntax.Inlines.Inline;

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
        // forca atualizacao dos titulos dos guias
        LocalizeGuides(LocalizationManager.CurrentCulture);
        
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
            MarkdownDocument markdownDocument = Markdig.Markdown.Parse(await sr.ReadToEndAsync(), pipeline);
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
    /// Returns a list of controls to be put into a stack panel making a guide
    /// </summary>
    /// <param name="guide">The guide to be processed</param>
    /// <returns>An ordered list of controls</returns>
    public List<Control> BuildGuide(GuideChapter guide) {
        string guideContent = GetLocalizedGuideContent(guide.GuideName);
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .Build();
        MarkdownDocument markdownDocument = Markdig.Markdown.Parse(guideContent, pipeline);

        List<Control> controls = [];

        foreach (Block block in markdownDocument) {
            if (block is YamlFrontMatterBlock) {
                continue;
            }
            
            switch (block) {
                case ParagraphBlock paragraphBlock: {
                    TextBlock textblock = new();
                    textblock.Classes.Add("paragraph");
                    
                    if (paragraphBlock.Inline is null) {
                        Console.WriteLine("Paragrafo sem inline");
                        break;
                    }

                    List<Inline> inlines = ParseInlines(paragraphBlock.Inline!);
                    textblock.Inlines ??= new InlineCollection();
                    foreach (Inline inline in inlines) {
                        textblock.Inlines.Add(inline);
                    }
                    controls.Add(textblock);
                    break;
                }
                case HeadingBlock headingBlock: {
                    var header = new TextBlock();
                    int level = headingBlock.Level;
                    if (level > 3) {
                        Console.WriteLine("Nao sei processar titulo maior que 3. Defaultando p/ 3");
                        level = 3;
                    }
                    header.Classes.Add("headerh"+level);
                    
                    header.Inlines ??= new InlineCollection();
                    header.Inlines.AddRange(ParseInlines(headingBlock.Inline!));
                    controls.Add(header);
                    break;
                }
                case CodeBlock codeBlock: {
                    Border border = new();
                    border.Classes.Add("codeblock");
                    SelectableTextBlock textblock = new();
                    textblock.Text = codeBlock.Lines.ToString();
                    textblock.Classes.Add("mono");
                    border.Child = textblock;
                    controls.Add(border);
                    break;
                }
                case QuoteBlock quoteBlock: {
                    TextBlock textblock = new();
                    textblock.Classes.Add("quote");
                    foreach (Block quoteContent in quoteBlock) {
                        if (quoteContent is not ParagraphBlock paragraph) {
                            Console.WriteLine("Nao sei processar bloco dentro de quote: "+quoteContent.GetType().FullName);
                            continue;
                        }
                        List<Inline> inlines = ParseInlines(paragraph.Inline!);
                        textblock.Inlines ??= new InlineCollection();
                        foreach (Inline inline in inlines) {
                            textblock.Inlines.Add(inline);
                        }
                    }
                    controls.Add(textblock);
                    break;
                }
                case LinkReferenceDefinitionGroup:
                    // Ignora, nao nos interessa. Queremos avisos importantes apenas
                    continue;
                default:
                    Console.WriteLine("Nao sei processar bloco do tipo: "+block.GetType().FullName);
                    break;
            }
        }

        return controls;
    }

    private static List<Inline> ParseInlines(ContainerInline container) {
        List<Inline> result = [];
        foreach (MdInline inline in container) {
            switch (inline) {
                case LiteralInline literal: {
                    result.Add(new Run(literal.Content.ToString()));
                    break;
                }
                case EmphasisInline emphasis: {
                    int delimiter = emphasis.DelimiterCount;
                    Span span = new();
                    if (delimiter is 1 or 3) {
                        span.Classes.Add("italic");
                    }
                    if (delimiter is 2 or 3) {
                        span.Classes.Add("bold");
                    }
                    foreach (Inline subInline in ParseInlines(emphasis)) {
                        span.Inlines.Add(subInline);
                    }
                    result.Add(span);
                    break;   
                }
                case CodeInline code: {
                    InlineUIContainer inlineContainer = new();
                    inlineContainer.Classes.Add("code");
                    Border border = new();
                    inlineContainer.Child = border;
                    TextBlock textBlock = new();
                    textBlock.Classes.Add("mono");
                    textBlock.Text = code.Content;
                    border.Child = textBlock;
                    result.Add(inlineContainer);
                    break;
                }
                case LineBreakInline:
                    result.Add(new Run(" "));
                    // isso se refere a um \n no markdown
                    // nao fica bonito, entao ignoramos
                    break;
                default:
                    Console.WriteLine("Nao sei processar Inline do tipo: "+inline.GetType().FullName);
                    break;
            }
        }
        return result;
    }
    
    [GeneratedRegex(@".\w\w-\w\w")]
    private static partial Regex CultureRemoverRegex();

    private class GuideMetadata {
        public string Title { get; set; } = "";
    }
}

