using Avalonia.Controls;
using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Editor.Models.Messages;
using Mercury.Editor.Views;
using Mercury.Editor.Views.CodeView;
using Mercury.Editor.Views.Design;
using Mercury.Editor.Views.ExecuteView;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor {
    
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            
            // initialize title bar
            TitleBar.Window = this;
            
            // initialize navigation
            nav = new Navigation(PageFrame);
            nav.Register<CodeTabView>(NavigationTarget.CodeView);
            nav.Register<ExecuteView>(NavigationTarget.ExecuteView, true);
            nav.Register<DesignView>(NavigationTarget.DesignView);
            nav.Navigate(NavigationTarget.CodeView);

            // initialize pop ups
            WeakReferenceMessenger.Default.Register<MainWindow, RequestTextPopupMessage>(this, static (recipient, msg) => {
                msg.Reply(recipient.TextPopup
                    .RequestAsync(msg)
                    .ContinueWith(r => r.Result));
            });
            WeakReferenceMessenger.Default.Register<MainWindow, RequestBoolPopupMessage>(this,
                static (recipient, msg) => {
                    msg.Reply(recipient.BoolPopup
                        .RequestAsync(msg)
                        .ContinueWith(r => r.Result));
                });
        }

        private readonly Navigation nav;

        private void OpenPreferences(object? sender, RoutedEventArgs e) {
            PreferencesView preferencesView = App.Services.GetRequiredService<PreferencesView>();
            preferencesView.ShowDialog(this);
        }

        private void OpenCode(object? sender, RoutedEventArgs e) {
            nav.Navigate(NavigationTarget.CodeView);
        }

        private void OpenRun(object? sender, RoutedEventArgs e) {
            nav.Navigate(NavigationTarget.ExecuteView);
        }

        private void OpenDesign(object? sender, RoutedEventArgs e) {
            nav.Navigate(NavigationTarget.DesignView);
        }

        private void OpenProjectConfiguration(object? sender, RoutedEventArgs e) {
            ProjectConfiguration config = App.Services.GetRequiredService<ProjectConfiguration>();
            config.ShowDialog(this);
        }

        private void OpenAbout(object? sender, RoutedEventArgs e) {
            AboutView about = App.Services.GetRequiredService<AboutView>();
            about.ShowDialog(this);
        }

        private void LogoClicked(object? sender, PointerPressedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://github.com/Agentew04/Mercury") { UseShellExecute = true });
        }
    }
}