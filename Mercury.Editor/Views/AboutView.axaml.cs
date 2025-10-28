using Avalonia.Controls;
using Mercury.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views;

public partial class AboutView : Window {
    public AboutView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<AboutViewModel>();
        ViewModel.SetView(this);
    }
    
    private AboutViewModel ViewModel { get; set; }
}