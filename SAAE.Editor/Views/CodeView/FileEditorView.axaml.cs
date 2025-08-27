using System.Xml;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using SAAE.Editor.ViewModels;
using SAAE.Editor.ViewModels.Code;
using FileEditorViewModel = SAAE.Editor.ViewModels.Code.FileEditorViewModel;

namespace SAAE.Editor.Views.CodeView;

public partial class FileEditorView : UserControl {

    private readonly GrammarService grammarService = App.Services.GetRequiredService<GrammarService>();
    
    public FileEditorView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<FileEditorViewModel>();
        ViewModel.SetView(this);
        WeakReferenceMessenger.Default.Register<FileOpenMessage>(this, OnFileOpen);
    }
    

    private static void OnFileOpen(object recipient, FileOpenMessage message) {
        FileEditorView view = (FileEditorView)recipient;
        // autofocus text editor
        view.TextEditor.Focus();
    }

    public FileEditorViewModel ViewModel { get; set; }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e) {
        TextEditor.SyntaxHighlighting = grammarService.GetCurrentAssemblyHighlighting();
    }

    private void File_PointerPressed(object? sender, PointerPressedEventArgs e) {
        PointerPoint point = e.GetCurrentPoint(sender as Control);
        if (!point.Properties.IsMiddleButtonPressed) return;
        if ((sender as Control)?.DataContext is OpenFile file) {
            ViewModel.CloseTabCommand.Execute(file);
        }
    }
}