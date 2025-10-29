using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Editor.Models;

public interface IGuidePart {

}

public partial class GuideMenu : ObservableObject, IGuidePart {
    [ObservableProperty] 
    private ObservableCollection<GuideChapter> availableChapters = [];

    [ObservableProperty] private ICommand openChapterCommand;

    public GuideMenu(ICommand openChapterCommand) {
        OpenChapterCommand = openChapterCommand;
    }
}

public partial class GuideChapter : ObservableObject, IGuidePart {
    
    public string GuideName { get; set; } = "";

    [ObservableProperty] private ICommand goBackCommand = null!;

    [ObservableProperty] private string localizedTitle = "";
    
    
    public GuideChapter() {
    }
}