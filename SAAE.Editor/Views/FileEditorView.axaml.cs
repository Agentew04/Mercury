using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class FileEditorView : UserControl {
    public FileEditorView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<FileEditorViewModel>();
    }
    
    public FileEditorViewModel ViewModel { get; set; }
}