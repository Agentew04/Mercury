using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.Views.ExecuteView;

public partial class OutputView : BaseControl<OutputView, OutputViewModel> {

    public ScrollViewer? OutputScroller { get; private set; }

    public OutputView() {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        OutputScroller = OutputTextBox.GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault();
        if (OutputScroller is null) {
            Logger.LogWarning("OutputTextBox ScrollView not found! Can't auto scroll to end!");
        }
    }

    private void OnSend(object? sender, RoutedEventArgs e) {
        InputTextBox.Focus();
    }
}