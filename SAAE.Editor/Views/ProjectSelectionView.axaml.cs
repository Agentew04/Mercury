using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class ProjectSelectionView : Window {
    public ProjectSelectionView() {
        InitializeComponent();
        DataContext = SelectionViewModel = App.Services.GetRequiredService<ProjectSelectionViewModel>();
        SelectionViewModel.view = this;
    }

    public ProjectSelectionViewModel SelectionViewModel { get; set; }
}