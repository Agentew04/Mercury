using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class AboutView : Window {
    public AboutView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<AboutViewModel>();
        ViewModel.SetView(this);
    }
    
    private AboutViewModel ViewModel { get; set; }
}