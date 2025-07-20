using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels.Execute;

namespace SAAE.Editor.Views.ExecuteView;

public partial class OutputView : UserControl {
    public OutputView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<OutputViewModel>();
    }

    public OutputViewModel ViewModel { get; init; }
}