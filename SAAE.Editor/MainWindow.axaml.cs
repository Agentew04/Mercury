using Avalonia.Controls;
using System;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Views;

namespace SAAE.Editor {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            TitleBar.Window = this;
            nav = new Navigation(TabControl);
            
            WeakReferenceMessenger.Default.Register<MainWindow, RequestTextPopupMessage>(this, static (recipient, msg) => {
                msg.Reply(recipient.TextPopup
                    .Request(msg.Title, msg.Watermark, msg.IsCancellable)
                    .ContinueWith(r => new TextPopupResult {
                        Result = r.Result ?? string.Empty,
                        IsCancelled = r.Result is null
                }));
            });
        }

        private readonly Navigation nav;

        private void OpenConfiguration(object? sender, RoutedEventArgs e) {
            ProjectConfiguration config = new();
            config.ShowDialog(this);
        }
    }
}