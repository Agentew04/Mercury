using Avalonia.Controls;
using Mercury.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views;

public partial class SplashScreen : BaseWindow<SplashScreen, SplashScreenViewModel> {
    public SplashScreen() {
        InitializeComponent();
    }
}