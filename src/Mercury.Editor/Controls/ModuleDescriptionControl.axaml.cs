using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Models.Modules;

namespace Mercury.Editor.Controls;

public partial class ModuleDescriptionControl : UserControl {
    
    public ModuleDescriptionControl() {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<ModuleDescription> ModuleDescriptionProperty = AvaloniaProperty.Register<ModuleDescriptionControl, ModuleDescription>(
        nameof(ModuleDescription));

    public ModuleDescription ModuleDescription {
        get => GetValue(ModuleDescriptionProperty);
        set => SetValue(ModuleDescriptionProperty, value);
    }
    
    public static readonly StyledProperty<RelayCommand<ModuleDescription>> DeleteCommandProperty = AvaloniaProperty.Register<ModuleDescriptionControl, RelayCommand<ModuleDescription>>(
        nameof(DeleteCommand));

    public RelayCommand<ModuleDescription> DeleteCommand {
        get => GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }
}