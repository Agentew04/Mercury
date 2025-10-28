using System;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Editor.Models.Messages;
using Mercury.Editor.Views;
using Mercury.Editor.Views.CodeView;
using Mercury.Editor.Views.ExecuteView;
using Mercury.Editor.Localization;
using Mercury.Editor.Models;

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
            nav.Navigate(NavigationTarget.CodeView);

            // initialize pop ups
            WeakReferenceMessenger.Default.Register<MainWindow, RequestTextPopupMessage>(this, static (recipient, msg) => {
                msg.Reply(recipient.TextPopup
                    .Request(msg)
                    .ContinueWith(r => r.Result));
            });
            WeakReferenceMessenger.Default.Register<MainWindow, RequestBoolPopupMessage>(this,
                static (recipient, msg) => {
                    msg.Reply(recipient.BoolPopup
                        .Request(msg)
                        .ContinueWith(r => r.Result));
                });
        }

        private readonly Navigation nav;

        private void OpenPreferences(object? sender, RoutedEventArgs e) {
            PreferencesView preferencesView = new();
            preferencesView.ShowDialog(this);
        }

        private void OpenCode(object? sender, RoutedEventArgs e) {
            nav.Navigate(NavigationTarget.CodeView);
        }

        private void OpenRun(object? sender, RoutedEventArgs e) {
            nav.Navigate(NavigationTarget.ExecuteView);
        }

        private void OpenProjectConfiguration(object? sender, RoutedEventArgs e) {
            ProjectConfiguration config = new();
            config.ShowDialog(this);
        }

        private void Open_About(object? sender, RoutedEventArgs e) {
            AboutView about = new();
            about.ShowDialog(this);
        }

        private void LogoClicked(object? sender, PointerPressedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://github.com/Agentew04/SAAE") { UseShellExecute = true });
        }
    }
}