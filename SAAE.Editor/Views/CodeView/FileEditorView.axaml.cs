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

    private void OnFileOpen(object recipient, FileOpenMessage message) {
        // autofocus text editor
        TextEditor.Focus();
    }

    public FileEditorViewModel ViewModel { get; set; }
}