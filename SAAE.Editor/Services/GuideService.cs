using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using SAAE.Editor.Models;
using Inline = Avalonia.Controls.Documents.Inline;

namespace SAAE.Editor.Services;

public class GuideService {
    private List<string> GetGuideManifestNames() {
        string[] allResources = typeof(GuideService).Assembly.GetManifestResourceNames();
        List<string> guideResources = allResources
            .Where(resource => resource.EndsWith(".md"))
            .Where(resource => resource.StartsWith("SAAE.Editor.Resources.Guide."))
            .ToList();
        return guideResources;
    }
    
    private IOrderedEnumerable<IGrouping<string, string>> availableLocalizedGuides;
    private Assembly assembly = typeof(GuideService).Assembly;
    
    private List<string> GetAvailableGuideNames() {
        List<GuideChapter> guideParts = [];
        List<string> guideResources = GetGuideManifestNames();
        availableLocalizedGuides = guideResources.GroupBy(x => x.Split('.')[^2]).OrderBy(x => x.Key);

        return availableLocalizedGuides.Select(x => x.Key).ToList();
    }

    private string GetGuideContent(string guideName) {
        string culture = Localization.LocalizationManager.CurrentCulture.ToString();
        using Stream? s = assembly.GetManifestResourceStream($"SAAE.Editor.Resources.Guide.{guideName}.{culture}.md");
        if (s is null) {
            return "";
        }

        using StreamReader sr = new(s);
        return sr.ReadToEnd();
    }

    public List<Inline> BuildGuide(string name) {
        string guideContent = GetGuideContent(name);
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        MarkdownDocument markdownDocument = Markdown.Parse(guideContent, pipeline);
        List<Inline> inlines = [];

        foreach (Block block in markdownDocument) {
            if (block is ParagraphBlock paragraphBlock) {
                inlines.AddRange(ParseInline(paragraphBlock.Inline));
                inlines.Add(new LineBreak()); // Adiciona uma quebra de linha entre parágrafos
            }else if (block is HeadingBlock headingBlock) {
                var headingText = new Run(GetInlineText(headingBlock.Inline))
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
}

