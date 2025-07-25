using System.ComponentModel;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;
using ProjectViewModel = SAAE.Editor.ViewModels.Code.ProjectViewModel;

namespace SAAE.Editor.Views.CodeView;

public partial class ProjectView : UserControl {
    public ProjectView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<ProjectViewModel>();
    }
    
    public ProjectViewModel ViewModel { get; set; }

    private void ContextMenu_OnOpening(object? sender, CancelEventArgs e) {
        
    }
}