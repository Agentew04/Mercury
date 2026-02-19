using System;
using Avalonia;
using Avalonia.Controls;
using Mercury.Editor.ViewModels.Code;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.CodeView;

public partial class CodeTabView : UserControl {
    public CodeTabView() {
        InitializeComponent();
        DataContext = vm = App.Services.GetRequiredService<CodeTabViewModel>();
        vm.LoadSizes(); // hopefully this happens before layout
        Unloaded += (_,_) => vm.OnUnload();
    }

    private CodeTabViewModel vm;
}