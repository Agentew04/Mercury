using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views;

public partial class SplashScreen : Window {
    public SplashScreen() {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<SplashScreenViewModel>();
        DataContext = ViewModel;
        ViewModel.SetView(this);
    }

    public SplashScreenViewModel ViewModel { get; private set; }
}