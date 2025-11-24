using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Mercury.Editor.Models;
using Mercury.Editor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Converters;

public sealed class GuideChapterDataTemplate : IDataTemplate, IDisposable {
    
    private readonly GuideService guideService = App.Services.GetService<GuideService>()!;

    private StackPanel? stackPanel;
    private GuideChapter? chapter; 
    
    public Control? Build(object? param) {
        stackPanel ??= new StackPanel();
        
        if(param is not GuideChapter guideChapter) {
            return null;
        }

        chapter = guideChapter;
        
        Localization.LocalizationManager.CultureChanged += UpdateLocalization;

        List<Control> controls = guideService.BuildGuide(chapter);
        stackPanel.Children.Clear();
        stackPanel.Children.AddRange(controls);
        
        return stackPanel;
    }

    private void UpdateLocalization(CultureInfo _) {
        List<Control> controls = guideService.BuildGuide(chapter!);
        stackPanel!.Children.Clear();
        stackPanel!.Children.AddRange(controls);
    }

    public bool Match(object? data) {
        return data is GuideChapter;
    }

    public void Dispose() {
        Localization.LocalizationManager.CultureChanged -= UpdateLocalization;
    }
}