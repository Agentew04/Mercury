using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class GuideView : UserControl {
    public GuideView() {
        InitializeComponent();
        ViewModel = App.Services.GetService<GuideViewModel>()!;
        DataContext = ViewModel;
    }
    
    public GuideViewModel ViewModel { get; private set; }
}