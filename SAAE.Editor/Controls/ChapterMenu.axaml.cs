using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;

namespace SAAE.Editor.Controls;

public class ChapterMenu : TemplatedControl {

    #region ChapterProperty

    public static readonly StyledProperty<GuideChapter> ChapterProperty =
        AvaloniaProperty.Register<ChapterMenu, GuideChapter>(nameof(Chapter));

    public GuideChapter Chapter {
        get => GetValue(ChapterProperty);
        set => SetValue(ChapterProperty, value);
    }

    #endregion

    #region GoBackCommandProperty

    public static readonly StyledProperty<ICommand> GoBackCommandProperty =
        AvaloniaProperty.Register<ChapterMenu, ICommand>(nameof(GoBackCommand));

    public ICommand GoBackCommand {
        get => GetValue(GoBackCommandProperty);
        set => SetValue(GoBackCommandProperty, value);
    }
    
    #endregion

    #region ContentProperty

    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<ChapterMenu, object>(nameof(Content));

    public object Content {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    #endregion
    
}