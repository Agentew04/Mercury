using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.ViewModels.Execute;

namespace SAAE.Editor.Views.ExecuteView;

public partial class OutputView : UserControl {

    private readonly ILogger<OutputView> logger = App.Services.GetRequiredService<ILogger<OutputView>>();

    public OutputView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<OutputViewModel>();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        ViewModel.OutputScroller = OutputTextBox.GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault();
        if (ViewModel.OutputScroller is null) {
            logger.LogWarning("OutputTextBox ScrollView not found! Can't auto scroll to end!");
        }
    }


    public OutputViewModel ViewModel { get; init; }

    private void OnSend(object? sender, RoutedEventArgs e) {
        InputTextBox.Focus();
    }
}