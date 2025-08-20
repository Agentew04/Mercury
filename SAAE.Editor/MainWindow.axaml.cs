using System;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SAAE.Editor.Localization;
using SAAE.Editor.Models;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Views;
using SAAE.Editor.Views.CodeView;
using SAAE.Editor.Views.ExecuteView;

namespace SAAE.Editor {
    
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            
            // initialize title bar
            TitleBar.Window = this;
            
            // initialize navigation
            nav = new Navigation(PageFrame);
            nav.Register<CodeTabView>(NavigationTarget.CodeView);
            nav.Register<ExecuteView>(NavigationTarget.ExecuteView);
            nav.Navigate(NavigationTarget.CodeView);
            
            // initialize context menu
            ProjectMenuOptions.Add(new MainContextOption {
                IsVisible = true,
                Command = OpenProjectSelectionCommand,
                Resource = MainWindowResources.Instance.GetOpenProjectSelectionContext
            });
            
            // initialize pop ups
            WeakReferenceMessenger.Default.Register<MainWindow, RequestTextPopupMessage>(this, static (recipient, msg) => {
                msg.Reply(recipient.TextPopup
                    .Request(msg.Title, msg.Watermark, msg.IsCancellable)
                    .ContinueWith(r => new TextPopupResult {
                        Result = r.Result ?? string.Empty,
                        IsCancelled = r.Result is null
                }));
            });
            WeakReferenceMessenger.Default.Register<MainWindow, RequestBoolPopupMessage>(this,
                static (recipient, msg) => {
                    msg.Reply(recipient.BoolPopup
                        .Request(msg.Title, msg.IsCancellable)
                        .ContinueWith(r => new BoolPopupResult {
                            Result = r.Result ??false,
                            IsCancelled = r.Result is null
                        }));
                });

            Task.Delay(10000)
                .ContinueWith(t => {
                    Dispatcher.UIThread.InvokeAsync(async () => {
                        for (int i = 0; i < 10; i++) {
                            Console.WriteLine("Mudando cultura");
                            if (LocalizationManager.CurrentCulture.Name == "en-US") {
                                LocalizationManager.CurrentCulture = new("pt-BR");
                            }
                            else {
                                LocalizationManager.CurrentCulture = new("en-US");
                            }

                            Console.WriteLine("Mudado cultura");
                            await Task.Delay(1000);
                            Console.WriteLine("dps delay");
                        }
                    });
                });
        }

        private readonly Navigation nav;

        public List<MainContextOption> ProjectMenuOptions { get; set; } = [];

        private void OpenConfiguration(object? sender, RoutedEventArgs e) {
            ProjectConfiguration config = new();
            config.ShowDialog(this);
        }

        private void OpenCode(object? sender, RoutedEventArgs e) {
            nav.Navigate(NavigationTarget.CodeView);
        }

        private void OpenRun(object? sender, RoutedEventArgs e) {
            nav.Navigate(NavigationTarget.ExecuteView);
        }

        [RelayCommand]
        private void OpenProjectSelection(object? _) {
            
        }

        private void ProjectMenuItem_OnClick(object? sender, RoutedEventArgs e) {
            
        }

        private void Open_About(object? sender, RoutedEventArgs e) {
            AboutView about = new();
            about.ShowDialog(this);
        }
    }
}