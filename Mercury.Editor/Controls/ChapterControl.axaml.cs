using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Mercury.Editor.Models;

namespace Mercury.Editor.Controls;

public class ChapterControl : TemplatedControl {

    public static readonly StyledProperty<GuideChapter> CurrentGuideProperty = AvaloniaProperty.Register<ChapterControl, GuideChapter>(
        nameof(CurrentGuide));

    public GuideChapter CurrentGuide {
        get => GetValue(CurrentGuideProperty);
        set => SetValue(CurrentGuideProperty, value);
    }

    public static readonly StyledProperty<ICommand> GoBackProperty = AvaloniaProperty.Register<ChapterControl, ICommand>(
        nameof(GoBack));

    public ICommand GoBack {
        get => GetValue(GoBackProperty);
        set => SetValue(GoBackProperty, value);
    }
}