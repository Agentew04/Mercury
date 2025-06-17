using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;
using SAAE.Editor.ViewModels.Code;

namespace SAAE.Editor.Views.CodeView;

public partial class FileEditorToolbarView : UserControl
{
    public FileEditorToolbarView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<FileEditorToolbarViewModel>();
    }
    
    public FileEditorToolbarViewModel ViewModel { get; set; }
}