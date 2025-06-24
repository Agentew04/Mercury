using Avalonia.Controls;
using System;
using Avalonia.LogicalTree;

namespace SAAE.Editor {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            TitleBar.Window = this;
            nav = new Navigation(TabControl);
        }

        private readonly Navigation nav;
    }
}