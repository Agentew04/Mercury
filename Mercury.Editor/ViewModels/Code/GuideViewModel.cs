using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Models;
using Mercury.Editor.Services;
using Mercury.Editor.Views.CodeView;
using Microsoft.Extensions.DependencyInjection;
using Mercury.Editor.Extensions;

namespace Mercury.Editor.ViewModels.Code;

public sealed partial class GuideViewModel : BaseViewModel<GuideViewModel, GuideView> {

    private readonly GuideService guideService = App.Services.GetService<GuideService>()!;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentGuide))]
    private ObservableCollection<GuideChapter> guideChapters;
    
    [ObservableProperty]
    private IGuidePart currentGuide;
    
    private GuideMenu guideMenu;
    
    public GuideViewModel() {
        ReadOnlyCollection<GuideChapter> guides = guideService.GetAvailableGuides();
        _ = guides.ForEachExt(x => x.GoBackCommand = GoToMenuCommand);
        GuideChapters = new ObservableCollection<GuideChapter>(guides);
            
        guideMenu = new GuideMenu(OpenGuideCommand) {
            AvailableChapters = GuideChapters
        };
        CurrentGuide = guideMenu;
    }
    
    [RelayCommand]
    private void OpenGuide(GuideChapter chapter) {
        CurrentGuide = chapter;
    }

    [RelayCommand]
    private void GoToMenu() {
        CurrentGuide = guideMenu;
    }
}