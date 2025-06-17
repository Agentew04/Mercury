using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SAAE.Editor.Views.CodeView;

public partial class CodeTabView : UserControl {
    public CodeTabView() {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e) {
        if (Localization.LocalizationManager.CurrentCulture.Name == "en-US") {
            Localization.LocalizationManager.CurrentCulture = new CultureInfo("pt-BR");
        }else {
            Localization.LocalizationManager.CurrentCulture = new CultureInfo("en-US");
        }
    }
}