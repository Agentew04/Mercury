using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;
using FileEditorViewModel = SAAE.Editor.ViewModels.Code.FileEditorViewModel;

namespace SAAE.Editor.Views.CodeView;

public partial class FileEditorView : UserControl {
    public FileEditorView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<FileEditorViewModel>();
    }
    
    public FileEditorViewModel ViewModel { get; set; }
}