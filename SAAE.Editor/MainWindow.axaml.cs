using Avalonia.Controls;
using System;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using SAAE.Editor.Views;

namespace SAAE.Editor {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            TitleBar.Window = this;
            nav = new Navigation(TabControl);
        }

        private readonly Navigation nav;

        private void OpenConfiguration(object? sender, RoutedEventArgs e) {
            ProjectConfiguration config = new();
            config.ShowDialog(this);
        }
    }
}