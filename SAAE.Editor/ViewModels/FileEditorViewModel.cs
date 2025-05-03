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
        Localization.LocalizationManager.CultureChanged += _ => {
            OnPropertyChanged(nameof(EditingNotice));
        };
    }
    
    [ObservableProperty]
    private ObservableCollection<OpenFile> openFiles = [];
    
    [ObservableProperty]
    private TextDocument textDocument = new();

    public string EditingNotice => string.Format(Localization.FileEditorResources.EditMessageValue, filename);
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(EditingNotice))]
    private string filename;

    [ObservableProperty] 
    private bool isReadonlyEditor = false;

    private void OnFileOpen(object sender, FileOpenMessage message) {
        // hackzinho, so os nos do stdlib sao readonly por enquanto
        string path = fileService.GetAbsolutePath(message.Value.Id, message.Value.IsEffectiveReadOnly);
        if (path == "") {
            Console.WriteLine("Nao foi possivel encontrar o arquivo");
            return;
        }
        string content = File.ReadAllText(path);
        TextDocument.Text = content;
        IsReadonlyEditor = message.Value.IsEffectiveReadOnly;
        Filename = message.Value.Name;
    }
}