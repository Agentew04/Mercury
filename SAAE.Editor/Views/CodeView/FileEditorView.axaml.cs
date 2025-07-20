using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.ViewModels;
using FileEditorViewModel = SAAE.Editor.ViewModels.Code.FileEditorViewModel;

namespace SAAE.Editor.Views.CodeView;

public partial class FileEditorView : UserControl {
    public FileEditorView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<FileEditorViewModel>();
        ViewModel.TextEditor = TextEditor; // HACK
        WeakReferenceMessenger.Default.Register<FileOpenMessage>(this, OnFileOpen);
    }

    private static void OnFileOpen(object recipient, FileOpenMessage message) {
        FileEditorView view = (FileEditorView)recipient;
        // autofocus text editor
        view.TextEditor.Focus();
    }

    public FileEditorViewModel ViewModel { get; set; }
}