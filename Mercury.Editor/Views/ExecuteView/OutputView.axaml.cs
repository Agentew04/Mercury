using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Mercury.Editor.ViewModels.Execute;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.Views.ExecuteView;

public partial class OutputView : UserControl {

    private readonly ILogger<OutputView> logger = App.Services.GetRequiredService<ILogger<OutputView>>();
    
    public ScrollViewer? OutputScroller { get; private set; }

    public OutputView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<OutputViewModel>();
        ViewModel.SetView(this);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        OutputScroller = OutputTextBox.GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault();
        if (OutputScroller is null) {
            logger.LogWarning("OutputTextBox ScrollView not found! Can't auto scroll to end!");
        }
    }


    public OutputViewModel ViewModel { get; init; }

    private void OnSend(object? sender, RoutedEventArgs e) {
        InputTextBox.Focus();
    }
}