using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class ProjectView : UserControl {
    public ProjectView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<ProjectViewModel>();
    }
    
    public ProjectViewModel ViewModel { get; set; }
}