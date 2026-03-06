using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Editor.ViewModels.Design;

namespace Mercury.Editor.Views.Design;

public partial class EditNodeView : BaseWindow<EditNodeView, EditNodeViewModel> {
    public EditNodeView() {
        InitializeComponent();
    }
}