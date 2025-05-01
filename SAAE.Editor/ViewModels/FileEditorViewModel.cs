using System;
using System.Collections.ObjectModel;
using System.IO;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;

namespace SAAE.Editor.ViewModels;

public partial class FileEditorViewModel : BaseViewModel {

    private readonly FileService fileService = App.Services.GetRequiredService<FileService>();
    
    public FileEditorViewModel() {
        OpenFiles = [
            new OpenFile() {
                Filename = "main.asm",
                Content = "abc",
                IsDirty = true
            },
            new OpenFile() {
                Filename = "geom.asm",
                Content = "def"
            }
        ];

        WeakReferenceMessenger.Default.Register<FileOpenMessage>(this, OnFileOpen);
    }
    
    [ObservableProperty]
    private ObservableCollection<OpenFile> openFiles = [];
    
    [ObservableProperty]
    private TextDocument textDocument = new();

    private void OnFileOpen(object sender, FileOpenMessage message) {
        Console.WriteLine("Recebi mensagem de arquivo aberto: " + message.Value.Name);
        string path = fileService.GetAbsolutePath(message.Value.Id);
        string content = File.ReadAllText(path);
        TextDocument.Text = content;
        Console.WriteLine("Path: " + path);
        Console.WriteLine("Content: " + content);
    }
}