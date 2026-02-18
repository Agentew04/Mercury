using System;
using Avalonia.Controls;

namespace Mercury.Editor.Views.CodeView;

public partial class CodeTabView : UserControl {
    public CodeTabView() {
        InitializeComponent();
        GuideView.SizeChanged += (sender, args) => Console.WriteLine($"Guide new size{args.NewSize}");
    }
}