using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Models.Compilation;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using SAAE.Engine;

namespace SAAE.Editor.ViewModels.Code;

public partial class FileEditorViewModel : BaseViewModel {

    private readonly FileService fileService = App.Services.GetRequiredService<FileService>();
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    private readonly ICompilerService compilerService = App.Services.GetRequiredKeyedService<ICompilerService>(Architecture.Mips);
    private readonly ILogger<FileEditorViewModel> logger = App.Services.GetRequiredService<ILogger<FileEditorViewModel>>();
    
    public FileEditorViewModel() {
        WeakReferenceMessenger.Default.Register<FileOpenMessage>(this, OnFileOpen);
        Localization.LocalizationManager.CultureChanged += _ => {
            OnPropertyChanged(nameof(EditingNotice));
        };
    }
    
    [ObservableProperty]
    private ObservableCollection<OpenFile> openFiles = [];
    
    [ObservableProperty]
    private int selectedTabIndex = 0;
    
    [ObservableProperty]
    private TextDocument textDocument = new();
 
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(EditingNotice))]
    private string filename = "";
    public string EditingNotice => string.Format(Localization.FileEditorResources.EditMessageValue, (object?)Filename);

    [ObservableProperty] 
    private bool isReadonlyEditor;

    private string currentPath = "";
    // private readonly DispatcherTimer compilationTimer;
    private Guid? compilationId;
    
    private void OnFileOpen(object sender, FileOpenMessage message) {
        // funcao chamada quando o usuario abre um arquivo pela aba do projeto
        string path = fileService.GetAbsolutePath(message.Value.Id);
        if (path == "") {
            logger.LogWarning("Nao foi possivel encontrar o path do arquivo {FileName}/{FileId}", message.Value.Name, message.Value.Id);
            return;
        }

        OpenFile file = new(message.Value.Name, path, CloseTabCommand, message.Value.IsEffectiveReadOnly);
        file.TextDocument.Text = File.ReadAllText(path);
        OpenFiles.Add(file);
        
        ChangeTab(file);
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        // funcao chamada quando o usuario clica em uma aba diferente
        if (value < 0 || value >= OpenFiles.Count)
        {
            return;
        }
        
        OpenFile openFile = OpenFiles[value];
        ChangeTab(openFile);
    }
    
    private void ChangeTab(OpenFile openFile)
    {
        logger.LogInformation("Changing tab to {FileName} ({FilePath})", openFile.Filename, openFile.Path);
        // desativa todos
        foreach (OpenFile file in OpenFiles)
        {
            file.IsActive = false;
        }
        // ativa o atual
        openFile.IsActive = true;
        Console.WriteLine(string.Join(", ", OpenFiles.Select(x => $"{x.Filename} = {x.IsActive}")));
        IsReadonlyEditor = openFile.IsReadonly;
        TextDocument = openFile.TextDocument;
    }

    [RelayCommand]
    private async Task SaveProject()
    {
        Console.WriteLine("Save project");
        // para cada arquivo aberto:
        //   - carrega conteudo
        //   - salva no disco
        foreach(OpenFile file in OpenFiles)
        {
            if (file.IsReadonly)
            {
                continue;
            }
            await File.WriteAllTextAsync(file.Path, file.TextDocument.Text);   
        }
    }

    [RelayCommand]
    private async Task BuildProject()
    {
        
    }

    [RelayCommand]
    private void RunProject()
    {
    }

    [RelayCommand]
    private void CloseTab(OpenFile file)
    {
        int selectedIndex = SelectedTabIndex;
        int index = OpenFiles.IndexOf(file);
        OpenFiles.Remove(file);
        if (selectedIndex == index)
        {
            // tem que trocar
            if (OpenFiles.Count == 0)
            {
                // nao tem mais arquivos abertos, limpa o editor
                TextDocument.Text = "";
                return;
            }
            // troca para o arquivo aberto mais proximo (index-1)
            if (index - 1 >= 0)
            {
                // troca pra ele
                ChangeTab(OpenFiles[index-1]);
            }
            else
            {
                // o fechado era o primeiro, tenta trocar pro index + 0(da direita)
                ChangeTab(OpenFiles[index]);
            }
        }
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

public partial class OpenFile : ObservableObject
{
    [ObservableProperty] private TextDocument textDocument;
    
    [ObservableProperty] private string filename;

    [ObservableProperty] private string path;
    
    public IRelayCommand<OpenFile> CloseFileCommand { get; init; }
    
    public bool IsReadonly { get; init; }

    [ObservableProperty] private bool isActive;
    
    public OpenFile(string filename, string path, IRelayCommand<OpenFile> closeFileCommand, bool isReadonly)
    {
        this.filename = filename;
        this.path = path;
        textDocument = new TextDocument();
        CloseFileCommand = closeFileCommand;
        IsReadonly = isReadonly;
    }
} 
