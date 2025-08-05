using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Localization;
using SAAE.Editor.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Inline = Avalonia.Controls.Documents.Inline;
using MdInline = Markdig.Syntax.Inlines.Inline;

namespace SAAE.Editor.Services;

public sealed partial class GuideService : BaseService<GuideService>, IDisposable {

    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();

    public GuideService() {
        LocalizationManager.CultureChanged += LocalizeGuides;
    }

    public void Dispose() {
        LocalizationManager.CultureChanged -= LocalizeGuides;
    }
    
    private readonly List<string> guideNames = [];
    private readonly Dictionary<string, GuideChapter> chapterDictionary = [];
    private readonly Dictionary<(string name, CultureInfo culture), PathObject> pathDictionary = [];
    private readonly Dictionary<(string name, CultureInfo culture), GuideMetadata> localizedMetadata = [];
    
    /// <summary>
    /// Initializes the service reading guides and storing structures.
    /// </summary>
    public async Task InitializeAsync() {
        Logger.LogInformation("Initializing Guides...");

        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            Logger.LogWarning("Couldn't load guides. No project loaded.");
            return;
        }

        // manifestos completos referente aos guias
        List<PathObject> guides = [];

        // agrega os guias comuns a todos
        IEnumerable<PathObject> common = Directory.EnumerateFiles(settingsService.GuideSettings.Common.ToString(), "*.md", 
            new EnumerationOptions {
                RecurseSubdirectories = false
            }).Select(x => x.ToFilePath());
        guides.AddRange(common);

        // agrega os guias da arquitetura atual
        GuideArchitecture? guideArch = settingsService.GuideSettings.Architectures.Find(x => x.Architecture == project.Architecture);
        if (guideArch is null) {
            Logger.LogWarning("Couldn't load guides. No match for architecture specific information found.");
            return;
        }
        IEnumerable<PathObject> archSpecifc = Directory.EnumerateFiles(guideArch.Path.ToString(), "*.md", 
            new EnumerationOptions {
                RecurseSubdirectories = false
            }).Select(x => x.ToFilePath());
        guides.AddRange(archSpecifc);

        // agrega os guias do sistema operacional atual
        GuideOs? guideOs = guideArch.Os.Find(x => x.Identifier == project.OperatingSystem.Identifier);
        if (guideOs is null) {
            Logger.LogWarning("Couldn't load guides. No match for operating system specific information found.");
            return;
        }
        IEnumerable<PathObject> osSpecific = Directory.EnumerateFiles(guideOs.Path.ToString(), "*.md",
            new EnumerationOptions {
                RecurseSubdirectories = false
            }).Select(x => x.ToFilePath());
        guides.AddRange(osSpecific);

        foreach (PathObject guide in guides) {
            // format filename.pt-BR   -> without .md already
            string fileName = guide.Filename; // without .md
            Regex cultureRemover = CultureRemoverRegex();
            // line below could be replaced with substring, ignore last 6 characters of string!
            string fileWithoutCulture = cultureRemover.Replace(fileName, "");
            bool repeated = guideNames.Contains(fileWithoutCulture);
            if (!repeated) {
                guideNames.Add(fileWithoutCulture);
            }

            var culture = new CultureInfo(cultureRemover.Match(fileName).Value.Replace(".", ""));
            pathDictionary.Add((fileWithoutCulture, culture), guide);

            if (repeated) {
                continue;
            }
            
            GuideChapter chapter = new() {
                GuideName = fileWithoutCulture,
            };
            chapterDictionary.Add(fileWithoutCulture, chapter);
        }

        await ReadMetadataAsync();
        // forca atualizacao dos titulos dos guias
        LocalizeGuides(LocalizationManager.CurrentCulture);
        
    }

    private void LocalizeGuides(CultureInfo cultureInfo) {
        foreach ((string name, GuideChapter chapter) in chapterDictionary) {
            GuideMetadata metadata = localizedMetadata[(name, cultureInfo)];
            chapter.LocalizedTitle = metadata.Title;
        }
    }

    private async Task ReadMetadataAsync() {
        foreach (((string name, CultureInfo culture) key, PathObject path) in pathDictionary) {
            FileStream fs;
            try {
                fs = File.OpenRead(path.ToString());
            }
            catch (Exception e) {
                Logger.LogError("Nao consegui ler manifesto '{name}.{culture}': {path}. Erro: {err}", key.name, key.culture.ToString(), path.ToString(), e.Message);
                continue;
            }

            using StreamReader sr = new(fs);
            MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .Build();
            MarkdownDocument markdownDocument = Markdig.Markdown.Parse(await sr.ReadToEndAsync(), pipeline);
            YamlFrontMatterBlock? yamlBlock = markdownDocument.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
            if (yamlBlock is null) {
                Logger.LogError("Nao achei Yaml Front Matter no markdown: {name}/{lang}", key.name, key.culture);
                continue;
            }
            string yaml = yamlBlock.Lines.ToString();
            IDeserializer deserializer = new StaticDeserializerBuilder(new YamlStaticContext())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            GuideMetadata metadata = deserializer.Deserialize<GuideMetadata>(yaml);
            localizedMetadata.Add(key, metadata);
            fs.Close();
        }
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
        CultureInfo culture = LocalizationManager.CurrentCulture;
        PathObject path = pathDictionary[(guideName, culture)];
        try {
            using FileStream fs = File.OpenRead(path.ToString());
            using StreamReader sr = new(fs);
            return sr.ReadToEnd();
        }
        catch (Exception e) {
            Logger.LogError("Could not read guide file: {file}. Error: {err}", path.ToString(), e.Message);
            return "";
        }

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
        foreach (Block block in markdownDocument)
        {
            Control? ctrl = ParseBlock(block);
            if (ctrl is not null) {
                controls.Add(ctrl);
            }
        }

        return controls;
    }

    private Control? ParseBlock(Block block)
    {
        List<Control> controls = [];
        if (block is YamlFrontMatterBlock) {
            return null;
        }
        switch (block) {
            case ParagraphBlock paragraphBlock: {
                TextBlock textblock = new();
                textblock.Classes.Add("paragraph");
                
                if (paragraphBlock.Inline is null) {
                    Logger.LogWarning("Achei paragrafo sem inline");
                    break;
                }

                List<Inline> inlines = ParseInlines(paragraphBlock.Inline!);
                textblock.Inlines ??= new InlineCollection();
                foreach (Inline inline in inlines) {
                    textblock.Inlines.Add(inline);
                }
                return textblock;
            }
            case HeadingBlock headingBlock: {
                TextBlock header = new();
                int level = headingBlock.Level;
                if (level > 3) {
                    Logger.LogInformation("Nao sei processar titulo maior que 3. Defaultando p/ 3");
                    level = 3;
                }
                header.Classes.Add("headerh"+level);
                
                header.Inlines ??= new InlineCollection();
                header.Inlines.AddRange(ParseInlines(headingBlock.Inline!));
                return header;
            }
            case CodeBlock codeBlock: {
                Border border = new();
                border.Classes.Add("codeblock");
                SelectableTextBlock textblock = new() {
                    Text = codeBlock.Lines.ToString()
                };
                textblock.Classes.Add("mono");
                border.Child = textblock;
                return border;
            }
            case QuoteBlock quoteBlock: {
                TextBlock textblock = new();
                textblock.Classes.Add("quote");
                foreach (Block quoteContent in quoteBlock) {
                    if (quoteContent is not ParagraphBlock paragraph) {
                        Logger.LogWarning("Nao sei processar bloco dentro de quote: {type}", quoteContent.GetType().FullName);
                        continue;
                    }
                    List<Inline> inlines = ParseInlines(paragraph.Inline!);
                    textblock.Inlines ??= new InlineCollection();
                    foreach (Inline inline in inlines) {
                        textblock.Inlines.Add(inline);
                    }
                }

                return textblock;
            }
            case LinkReferenceDefinitionGroup:
                // Ignora, nao nos interessa. Queremos avisos importantes apenas
                break;
            case Table table: {
                Grid grid = new();
                grid.Classes.Add("table");
                int columnCount = table.ColumnDefinitions.Count - 1;
                for (int i = 0; i < columnCount; i++)
                {
                    grid.ColumnDefinitions.Add(table.ColumnDefinitions[i].Width is 0 ? new ColumnDefinition(GridLength.Star) : new ColumnDefinition(table.ColumnDefinitions[i].Width, GridUnitType.Star));
                }

                for (int row = 0; row < table.Count; row++)
                {
                    TableRow tableRow = (TableRow)table[row];
                    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

                    for (int col = 0; col < columnCount; col++)
                    {
                        TableCell cell = (TableCell)tableRow[col];
                        Border border = new();
                        border.Classes.Add("tablecell");
                        if (row == 0)
                        {
                            border.Classes.Add("tableheader");
                        }
                        grid.Children.Add(border);
                        Grid.SetRow(border, row);
                        Grid.SetColumn(border, col);
                        if (cell.Count > 1)
                        {
                            StackPanel stackPanel = new()
                            {
                                Orientation = Orientation.Vertical
                            };
                            border.Child = stackPanel;
                            foreach (Block child in cell)
                            {
                                Control? ctrl = ParseBlock(child);
                                if (ctrl is not null)
                                {
                                    stackPanel.Children.Add(ctrl);
                                }
                            }
                        }
                        else if(cell.Count == 1)
                        {
                            Control? ctrl = ParseBlock(cell[0]);
                            if (ctrl is not null)
                            {
                                border.Child = ctrl;
                            }
                        }
                    }
                }

                return grid;
            }
            default:
                Logger.LogWarning("Nao sei processar bloco do tipo: {type}", block.GetType().FullName);
                break;
        }
        return null;
    }

    private List<Inline> ParseInlines(ContainerInline container) {
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
                    Logger.LogWarning("Nao sei processar Inline do tipo: {type}", inline.GetType().FullName);
                    break;
            }
        }
        return result;
    }
    
    [GeneratedRegex(@"\.\w\w-\w\w")]
    private static partial Regex CultureRemoverRegex();

}
public class GuideMetadata {
    public string Title { get; set; } = "";
}

[YamlStaticContext]
[YamlSerializable(typeof(GuideMetadata))]
public partial class YamlStaticContext;

