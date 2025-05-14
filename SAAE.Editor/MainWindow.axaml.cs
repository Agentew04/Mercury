using Avalonia.Controls;
using System;

namespace SAAE.Editor {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            TitleBar.Window = this;
        }
    }
}