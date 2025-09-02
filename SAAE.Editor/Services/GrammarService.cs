using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Models;
using SAAE.Engine;
using SAAE.Engine.Common;

namespace SAAE.Editor.Services;

public class GrammarService : BaseService<GrammarService> {

    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    private readonly Assembly assembly;

    public GrammarService() {
        assembly = Assembly.GetExecutingAssembly();
    }

    private Dictionary<Architecture, IHighlightingDefinition> highlightCache = [];
    
    public IHighlightingDefinition? GetCurrentAssemblyHighlighting() {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            Logger.LogWarning("Could not find project loaded to get assembly highlighting");
            return null;
        }

        if (highlightCache.TryGetValue(project.Architecture, out IHighlightingDefinition? cachedHighlight)) {
            return cachedHighlight;
        }

        Stream? s;
        switch (project.Architecture) {
            case Architecture.Mips:
                s = assembly.GetManifestResourceStream("SAAE.Editor.Assets.Grammars.mips.xshd");
                break;
            case Architecture.Arm:
            case Architecture.RiscV:
            case Architecture.Unknown:
            default:
                Logger.LogWarning("Tried to fetch unsupported syntax highlighting: {arch}", project.Architecture);
                return null;
        }
        
        if(s is null) {
            return null;
        }

        using var reader = XmlReader.Create(s);
        IHighlightingDefinition? highlight = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        s.Dispose();
        highlightCache[project.Architecture] = highlight;
        return highlight;
    }
    
}