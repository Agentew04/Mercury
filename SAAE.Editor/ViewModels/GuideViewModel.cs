using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;

namespace SAAE.Editor.ViewModels;

public partial class GuideViewModel : BaseViewModel {

    private readonly GuideService _guideService = App.Services.GetService<GuideService>()!;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentGuide))]
    private ObservableCollection<GuideChapter> guideChapters;
    
    [ObservableProperty]
    private IGuidePart currentGuide;
    
    private GuideMenu guideMenu;
    
    public GuideViewModel() {
        ReadOnlyCollection<GuideChapter> guides = _guideService.GetAvailableGuides();
        _ = guides.ForEach(x => x.GoBackCommand = GoToMenuCommand);
        GuideChapters = new ObservableCollection<GuideChapter>(guides);
            
        guideMenu = new GuideMenu(OpenGuideCommand) {
            AvailableChapters = GuideChapters
        };
        CurrentGuide = guideMenu;
    }

    [RelayCommand]
    public void OpenGuide(GuideChapter chapter) {
        CurrentGuide = chapter;
    }

    [RelayCommand]
    public void GoToMenu() {
        CurrentGuide = guideMenu;
    }
}