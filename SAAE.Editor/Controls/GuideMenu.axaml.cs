using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using SAAE.Editor.Models;

namespace SAAE.Editor.Controls;

public class GuideMenu : TemplatedControl {

    #region ChaptersProperty

    public static readonly StyledProperty<ObservableCollection<GuideChapter>> ChaptersProperty =
        AvaloniaProperty.Register<GuideMenu, ObservableCollection<GuideChapter>>(nameof(Chapters));

    public ObservableCollection<GuideChapter> Chapters
    {
        get => GetValue(ChaptersProperty);
        set => SetValue(ChaptersProperty, value);
    }

    #endregion

    #region OpenGuideCommandProperty

    public static readonly StyledProperty<ICommand> OpenGuideCommandProperty =
        AvaloniaProperty.Register<GuideMenu, ICommand>(nameof(OpenGuideCommand));

    public ICommand OpenGuideCommand {
        get => GetValue(OpenGuideCommandProperty);
        set => SetValue(OpenGuideCommandProperty, value);
    }

    #endregion
}