using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
        var path = fileService.GetAbsolutePath(message.Value.Id, message.Value.IsEffectiveReadOnly);
        if (path == "") {
            Console.WriteLine("Nao foi possivel encontrar o arquivo");
            return;
        }

        currentPath = path;
        var content = File.ReadAllText(path);
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
            
            var input = fileService.CreateCompilationInput();
            
            // verifica se o arquivo foi alterado
            if (compilationId is null)
            {
                // primeira compilacao
                await Compile(input);
                return;
            }

            var currentId = input.CalculateId(projectService.GetCurrentProject()!.ProjectDirectory,
                MipsCompiler.EntryPointPreambule);

            if (currentId == compilationId)
            {
                // nao precisa compilar, nada mudou
                return;
            }
            
            // algo mudou, recompila
            await Compile(input);
        }
        catch (Exception)
        {
            Debug.Fail("Houve um erro interno na compilacao automatica!");
        }
    }
    
    private async ValueTask Compile(CompilationInput input)
    {
        var result = await compilerService.CompileAsync(input);
        compilationId = result.Id;
        WeakReferenceMessenger.Default.Send(new CompilationFinishedMessage(result.Id));
    }
}