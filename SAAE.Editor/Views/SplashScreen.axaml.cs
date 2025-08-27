using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class SplashScreen : Window {
    public SplashScreen() {
        InitializeComponent();
        ViewModel = App.Services.GetService<SplashScreenViewModel>() ?? throw new Exception("Could not resolve service");
        DataContext = ViewModel;
        ViewModel.SetView(this);
    }

    public SplashScreenViewModel ViewModel { get; private set; }
}