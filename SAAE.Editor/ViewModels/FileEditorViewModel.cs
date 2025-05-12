using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Models.Compilation;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using SAAE.Engine;

namespace SAAE.Editor.ViewModels;

public partial class FileEditorViewModel : BaseViewModel {

    private readonly FileService fileService = App.Services.GetRequiredService<FileService>();
    private readonly ICompilerService compilerService = App.Services.GetRequiredKeyedService<ICompilerService>(Architecture.Mips);
    
    public FileEditorViewModel() {
        WeakReferenceMessenger.Default.Register<FileOpenMessage>(this, OnFileOpen);
        Localization.LocalizationManager.CultureChanged += _ => {
            OnPropertyChanged(nameof(EditingNotice));
        };
        compilationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        //compilationTimer.Tick += OnCompilationTimerTick;
        //compilationTimer.Start();
    }
    
    [ObservableProperty]
    private TextDocument textDocument = new();
 
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(EditingNotice))]
    private string filename = "";
    public string EditingNotice => string.Format(Localization.FileEditorResources.EditMessageValue, Filename);

    [ObservableProperty] 
    private bool isReadonlyEditor;

    
    private string currentPath = "";
    private readonly DispatcherTimer compilationTimer;
    private byte[]? compilationHash;
    
    
    private void OnFileOpen(object sender, FileOpenMessage message) {
        // hackzinho, so os nos do stdlib sao readonly por enquanto
        string path = fileService.GetAbsolutePath(message.Value.Id, message.Value.IsEffectiveReadOnly);
        if (path == "") {
            Console.WriteLine("Nao foi possivel encontrar o arquivo");
            return;
        }

        currentPath = path;
        string content = File.ReadAllText(path);
        TextDocument.Text = content;
        IsReadonlyEditor = message.Value.IsEffectiveReadOnly;
        Filename = message.Value.Name;
    }

    [RelayCommand]
    private async Task Save()
    {
        var content = TextDocument.Text;
        await File.WriteAllTextAsync(currentPath, content);
    }

    private async void OnCompilationTimerTick(object? sender, EventArgs e)
    {
        try
        {
            // verifica se tem arquivo aberto
            if (currentPath == "" || Filename == "")
            {
                return;
            }
        
            // verifica se o arquivo foi alterado
            if (compilationHash is null)
            {
                // primeira compilacao
                await Compile();
            }
        }
        catch (Exception ex)
        {
            throw; // TODO handle exception
        }
    }
    
    private async Task Compile()
    {
        //var project = fileService.GetProjectFiles();
        
        CompilationInput input = new CompilationInput()
        {
        };
        var result = await compilerService.CompileAsync(input);

        WeakReferenceMessenger.Default.Send(new CompilationFinishedMessage(result.Id));
    }
}