using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;

namespace SAAE.Editor.Converters;

public class GuideChapterDataTemplate : IDataTemplate {
    
    private readonly GuideService _guideService = App.Services.GetService<GuideService>()!;
    
    public Control? Build(object? param) {
        TextBlock textBlock = new();
        
        if(param is not GuideChapter chapter) {
            return null;
        }

        InlineCollection inlineCollection = textBlock.Inlines ?? new InlineCollection();
        inlineCollection.AddRange(_guideService.BuildInlines(chapter));
        return textBlock;
    }

    public bool Match(object? data) {
        return data is GuideChapter;
    }
}