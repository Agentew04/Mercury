using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Converters;
using SAAE.Editor.Models;
using SAAE.Editor.Services;

namespace SAAE.Editor.Controls;

public partial class ChapterView : UserControl {

    private readonly GuideService _guideService = App.Services.GetService<GuideService>()!;
    
    public ChapterView() {
        InitializeComponent();
        DataContext = this;
        Localization.LocalizationManager.CultureChanged += UpdateCulture;
    }

    private void UpdateCulture(CultureInfo cultureInfo) {
        GuideChapter chapter = CurrentChapter;
        CurrentChapter = null!;
        CurrentChapter = chapter;
    }
    
    #region CurrentChapter Property

    public static readonly StyledProperty<GuideChapter> CurrentChapterProperty =
        AvaloniaProperty.Register<ChapterView, GuideChapter>(nameof(CurrentChapter));
    
    public GuideChapter CurrentChapter {
        get => GetValue(CurrentChapterProperty);
        set => SetValue(CurrentChapterProperty, value);
    }
    
    #endregion
    
}