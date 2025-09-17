using System.Collections.Generic;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
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
        TextEditor.TextArea.TextEntering += TextEntering;
        TextEditor.TextArea.TextEntered += TextEntered;
    }
    

    private static void OnFileOpen(object recipient, FileOpenMessage message) {
        FileEditorView view = (FileEditorView)recipient;
        // autofocus text editor
        view.TextEditor.Focus();
    }

    public FileEditorViewModel ViewModel { get; set; }
    private CompletionWindow? completionWindow;

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
    
    private void TextEntering(object? sender, TextInputEventArgs e) {
        if (e.Text == ".") {
            completionWindow = new CompletionWindow(TextEditor.TextArea);
            IList<ICompletionData>? data = completionWindow.CompletionList.CompletionData;
            data.Add(new TextCompletionData(".macro"));
            data.Add(new TextCompletionData(".text"));
            data.Add(new TextCompletionData(".data"));
            data.Add(new TextCompletionData(".ascii"));
            data.Add(new TextCompletionData(".asciiz"));
            data.Add(new TextCompletionData(".word"));
            data.Add(new TextCompletionData(".space"));
            data.Add(new TextCompletionData(".globl"));
            data.Add(new TextCompletionData(".align"));
            data.Add(new TextCompletionData(".float"));
            data.Add(new TextCompletionData(".double"));
            data.Add(new TextCompletionData(".endmacro"));
            completionWindow.Show();
            completionWindow.Closed += delegate {
                completionWindow = null;
            };
        }
    }

    private void TextEntered(object? sender, TextInputEventArgs e) {
        if(e.Text?.Length > 0 && completionWindow != null) {
            if(!char.IsLetterOrDigit(e.Text[0])) {
                //completionWindow.CompletionList.RequestInsertion(e);
            }
        }
    }
}