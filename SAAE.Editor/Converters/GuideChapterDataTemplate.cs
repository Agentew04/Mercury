using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;

namespace SAAE.Editor.Converters;

public class GuideChapterDataTemplate : IDataTemplate, IDisposable {
    
    private readonly GuideService _guideService = App.Services.GetService<GuideService>()!;

    private StackPanel? stackPanel = null;
    private GuideChapter? chapter = null; 
    
    public Control? Build(object? param) {
        stackPanel ??= new StackPanel();
        
        if(param is not GuideChapter guideChapter) {
            return null;
        }

        chapter = guideChapter;
        
        Localization.LocalizationManager.CultureChanged += UpdateLocalization;

        List<Control> controls = _guideService.BuildGuide(chapter);
        stackPanel.Children.Clear();
        stackPanel.Children.AddRange(controls);
        
        return stackPanel;
    }

    private void UpdateLocalization(CultureInfo _) {
        List<Control> controls = _guideService.BuildGuide(chapter!);
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