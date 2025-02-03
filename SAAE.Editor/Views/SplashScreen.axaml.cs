using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class SplashScreen : Window {
    public SplashScreen() {
        InitializeComponent();
        ViewModel = new SplashScreenViewModel();
        DataContext = ViewModel;
    }

    public SplashScreenViewModel ViewModel { get; private set; }
}