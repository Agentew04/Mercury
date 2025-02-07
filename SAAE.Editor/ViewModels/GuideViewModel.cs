using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAAE.Editor.Models;

namespace SAAE.Editor.ViewModels;

public partial class GuideViewModel : BaseViewModel {

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentGuide))]
    private ObservableCollection<GuideChapter> guideChapters;
    
    [ObservableProperty]
    private IGuidePart currentGuide;
    
    private GuideMenu guideMenu;
    
    public GuideViewModel() {
        GuideChapters = [
            new GuideChapter(GoToMenuCommand) {
                Title = "For Loops"
            },

            new GuideChapter(GoToMenuCommand) {
                Title = "If Statement"
            },

            new GuideChapter(GoToMenuCommand) {
                Title = "Functions"
            },

            new GuideChapter(GoToMenuCommand) {
                Title = "Registers"
            },

            new GuideChapter(GoToMenuCommand) {
                Title = "Memory"
            }
        ];

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