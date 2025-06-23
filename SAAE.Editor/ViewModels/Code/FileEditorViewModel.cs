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
        // para cada arquivo aberto:
        //   - carrega conteudo
        //   - salva no disco
        logger.LogInformation("Saving project with {FileCount} open files", OpenFiles.Count);
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
        // salva projeto caso o usuario nao tenha salvo
        await SaveProject();

        logger.LogInformation("Building project");
        CompilationInput input = fileService.CreateCompilationInput();
        WeakReferenceMessenger.Default.Send(
            new CompilationStartedMessage(input.CalculateId(MipsCompiler.EntryPointPreambule)));
        CompilationResult result = await compilerService.CompileAsync(input);
        WeakReferenceMessenger.Default.Send(new CompilationFinishedMessage(result.Id));
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
        
        // salvar arquivo ao fechar
        if (!file.IsReadonly)
        {
            // salva o conteudo no disco
            File.WriteAllText(file.Path, file.TextDocument.Text);
        }
        
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
