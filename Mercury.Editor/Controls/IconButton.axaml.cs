using Avalonia;
using Avalonia.Controls.Primitives;

namespace Mercury.Editor.Controls;

public class IconButton : TemplatedControl {

    public static readonly StyledProperty<string> IconProperty = AvaloniaProperty.Register<IconButton, string>(
        nameof(Icon));

    public string Icon {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}