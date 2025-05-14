using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models.Compilation;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using SAAE.Engine;

namespace SAAE.Editor.ViewModels;

public partial class FileEditorViewModel : BaseViewModel {

    private readonly FileService fileService = App.Services.GetRequiredService<FileService>();
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
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
        compilationTimer.Tick += OnCompilationTimerTick;
        compilationTimer.Start();
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
    private Guid? compilationId;
    
    
    private void OnFileOpen(object sender, FileOpenMessage message) {
        // hackzinho, so os nos do stdlib sao readonly por enquanto
        string path = fileService.GetAbsolutePath(message.Value.Id);
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

    public KeyGesture SaveAllGesture => new KeyGesture(Key.S, KeyModifiers.Control | KeyModifiers.Shift);

    [RelayCommand]
    private async Task SaveAll()
    {
        Console.WriteLine("Save All");
    }
    
    public KeyGesture SaveCurrentGesture => new KeyGesture(Key.S, KeyModifiers.Control);
        
    [RelayCommand]
    private async Task SaveCurrent()
    {
        Console.WriteLine("Save Current");
        string? content = TextDocument.Text;
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
            
            CompilationInput input = fileService.CreateCompilationInput();
            
            // verifica se o arquivo foi alterado
            if (compilationId is null)
            {
                // primeira compilacao
                await Compile(input);
                return;
            }

            Guid currentId = input.CalculateId(MipsCompiler.EntryPointPreambule);

            if (currentId == compilationId)
            {
                // nao precisa compilar, nada mudou
                return;
            }
            
            // algo mudou, recompila
            await Compile(input);
        }
        catch (Exception ex)
        {
            Debug.Fail("Houve um erro interno na compilacao automatica! " + ex.Message + "; " + ex.StackTrace);
        }
    }
    
    private async ValueTask Compile(CompilationInput input)
    {
        WeakReferenceMessenger.Default.Send(
            new CompilationStartedMessage(input.CalculateId(MipsCompiler.EntryPointPreambule)));
        Console.WriteLine("Compiling...");
        CompilationResult result = await compilerService.CompileAsync(input);
        Console.WriteLine($"Compilado {(result.IsSuccess ? "com sucesso" : $"com erro ({result.Diagnostics?.Count ?? -1} diagnosticos)")}");
        compilationId = result.Id;
        WeakReferenceMessenger.Default.Send(new CompilationFinishedMessage(result.Id));
    }
}