using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Models;
using SAAE.Editor.Models.Compilation;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using SAAE.Editor.Views.CodeView;
using SAAE.Engine;

namespace SAAE.Editor.ViewModels.Code;

public partial class FileEditorViewModel : BaseViewModel<FileEditorViewModel, FileEditorView> {

    private readonly FileService fileService = App.Services.GetRequiredService<FileService>();
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    private readonly ICompilerService compilerService = App.Services.GetRequiredKeyedService<ICompilerService>(Architecture.Mips);
    private readonly ExecuteService executeService = App.Services.GetRequiredService<ExecuteService>();
    
    public FileEditorViewModel() {
        WeakReferenceMessenger.Default.Register<FileEditorViewModel,FileOpenMessage>(this, OnFileOpen);
        WeakReferenceMessenger.Default.Register<FileEditorViewModel,ProgramLoadMessage>(this, OnProgramLoad);
        WeakReferenceMessenger.Default.Register<FileEditorViewModel,FileDeleteMessage>(this, OnFileDelete);
        WeakReferenceMessenger.Default.Register<FileEditorViewModel,FileMoveMessage>(this, OnFileMove);

        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            Logger.LogWarning("Ordem errada! project service not initialized");
            return;
        }
        OnFileOpen(this, new FileOpenMessage {
            Path = project.ProjectDirectory + project.SourceDirectory + project.EntryFile,
            LineNumber = 1,
            ColumnNumber = 1
        });
    }

    #region Editor Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOpenFiles))]
    private ObservableCollection<OpenFile> openFiles = [];

    public bool HasOpenFiles {
        get {
            Logger.LogDebug("Open files? {res}", OpenFiles.Count > 0);
            return OpenFiles.Count > 0;
        }
    }

    [ObservableProperty]
    private int selectedTabIndex;
    
    [ObservableProperty]
    private TextDocument textDocument = new();
    
    [ObservableProperty] 
    private bool isReadonlyEditor;

    #endregion
    
    #region Toolbar Properties
    
    [ObservableProperty]
    private bool canRunProject = false;
    
    #endregion

    private static void OnFileDelete(FileEditorViewModel recipient, FileDeleteMessage msg) {
        OpenFile? file = recipient.OpenFiles
            .FirstOrDefault(x =>
                x.Path == recipient.fileService.GetAbsolutePath(msg.ProjectNode.Id));
        if (file is null) {
            return;
        }
        recipient.CloseTab(file);
    }

    private static void OnFileMove(FileEditorViewModel vm, FileMoveMessage msg) {
        foreach (OpenFile file in vm.OpenFiles) {
            if (file.Path != msg.OldPath) {
                continue;
            }
            file.Path = msg.NewPath;
        }
    }

    private static void OnFileOpen(FileEditorViewModel vm, FileOpenMessage message) {
        // funcao chamada quando o usuario abre um arquivo pela aba do projeto
        PathObject path;
        int? line = message.LineNumber;
        int? column = message.ColumnNumber;
        if (message.ProjectNode is not null)
        {
            // abriu do project view
            path = vm.fileService.GetAbsolutePath(message.ProjectNode.Id);
            if (path == default)
            {
                vm.Logger.LogWarning("Nao foi possivel encontrar o path do arquivo {FileName}/{FileId}", message.ProjectNode.Name, message.ProjectNode.Id);
                return;
            }
        }
        else
        {
            // abriu do problems view
            path = message.Path!.Value;
        }
        
        // verifica se o arquivo ja esta aberto
        OpenFile? existingFile = vm.OpenFiles.FirstOrDefault(x => x.Path == path);
        if (existingFile is not null)
        {
            vm.ChangeTab(existingFile, line, column);
            return;
        }
        
        string name = path.FullFileName;

        // message.ProjectNode?.IsEffectiveReadOnly ?? false pois a stdlib nunca deveria emitir um warning ou erro!!!
        OpenFile file = new(name, path, vm.CloseTabCommand, message.ProjectNode?.IsEffectiveReadOnly ?? false);
        file.TextDocument.Text = File.ReadAllText(path.ToString());
        vm.OpenFiles.Add(file);
        vm.OnPropertyChanged(nameof(HasOpenFiles));
        vm.ChangeTab(file, line, column);
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

    private static void OnProgramLoad(FileEditorViewModel recipient, ProgramLoadMessage message)
    {
        // chamada quando o programa compilado eh carregado em uma maquina
        recipient.CanRunProject = true;
    }

    private void ChangeTab(OpenFile openFile, int? line = null, int? column = null)
    {
        // desativa todos
        foreach (OpenFile file in OpenFiles)
        {
            file.IsActive = false;
        }
        // ativa o atual
        openFile.IsActive = true;
        IsReadonlyEditor = openFile.IsReadonly;
        TextDocument = openFile.TextDocument;
        
        // atualiza o cursor
        UpdateCursor(line, column);
        
        Logger.LogInformation("Changing Editor Tab to [{Index}] {FileName} ({FilePath})", 
            OpenFiles.IndexOf(openFile), 
            openFile.Filename, 
            openFile.Path);
        SelectedTabIndex = OpenFiles.IndexOf(openFile);
    }

    private void UpdateCursor(int? lineNumber, int? columnNumber)
    {
        if (lineNumber is null && columnNumber is null) {
            return;
        }
        
        if (lineNumber is not null && TextDocument.LineCount < lineNumber)
        {
            Logger.LogWarning("Line number {Line} is out of bounds for the document with {LineCount} lines", lineNumber, TextDocument.LineCount);
            return;
        }

        FileEditorView? view = GetView();
        DocumentLine line = lineNumber is not null 
            ? TextDocument.GetLineByNumber(lineNumber.Value) : 
            TextDocument.GetLineByOffset(view?.TextEditor.CaretOffset ?? 0); 
        
        int column = columnNumber ?? 1; // se nao tiver coluna, usa a primeira
        if (line.Length < column)
        {
            Logger.LogWarning("Column number {Column} is out of bounds for the line with {Length} characters", column, line.Length);
            column = line.Length; // ajusta para o tamanho da linha
        }
        
        // atualiza o cursor
        // por algum motivo o offset do texteditor nao aceita bindings
        //CursorOffset = line.Offset + column - 1; // -1 porque o caret offset eh zero-based
        if (view?.TextEditor != null) {
            view.TextEditor.CaretOffset = line.Offset + column - 1;
        }
    }

    [RelayCommand]
    private async Task SaveProject()
    {
        // para cada arquivo aberto:
        //   - carrega conteudo
        //   - salva no disco
        Logger.LogInformation("Saving project with {FileCount} open files", OpenFiles.Count);
        int changedFiles = 0;
        foreach(OpenFile file in OpenFiles)
        {
            if (file.IsReadonly)
            {
                continue;
            }
            changedFiles++;
            await File.WriteAllTextAsync(file.Path.ToString(), file.TextDocument.Text);   
        }

        if (changedFiles > 0)
        {
            CanRunProject = false;
        }
    }

    [RelayCommand]
    private async Task BuildProject()
    {
        // salva projeto caso o usuario nao tenha salvo
        await SaveProject();

        Logger.LogInformation("Building project");
        CompilationInput input = fileService.CreateCompilationInput();
        WeakReferenceMessenger.Default.Send(
            new CompilationStartedMessage(input.CalculateId()));
        CompilationResult result = await compilerService.CompileAsync(input);
        WeakReferenceMessenger.Default.Send(new CompilationFinishedMessage(result));
        Logger.LogInformation("Compilation Finished. sucess? {res}", result.IsSuccess);
        if (result.IsSuccess) {
            CanRunProject = true;
        }
    }

    [RelayCommand]
    private void RunProject()
    {
        // load program
        executeService.LoadProgram();
        
        // navigate automagically to the execution view
        Navigation.NavigateTo(NavigationTarget.ExecuteView);
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
            // salva o conteudo no disco se o arquivo ainda existe
            if (File.Exists(file.Path.ToString())) {
                File.WriteAllText(file.Path.ToString(), file.TextDocument.Text);
            }
        }
        
        if (selectedIndex == index)
        {
            // tem que trocar
            if (OpenFiles.Count == 0)
            {
                // nao tem mais arquivos abertos, limpa o editor
                TextDocument.Text = "";
                OnPropertyChanged(nameof(HasOpenFiles));
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
        OnPropertyChanged(nameof(HasOpenFiles));
    }
}

public partial class OpenFile : ObservableObject
{
    [ObservableProperty] private TextDocument textDocument;
    
    [ObservableProperty] private string filename;

    [ObservableProperty] private PathObject path;
    
    public IRelayCommand<OpenFile> CloseFileCommand { get; init; }
    
    public bool IsReadonly { get; init; }

    [ObservableProperty] private bool isActive;
    
    public OpenFile(string filename, PathObject path, IRelayCommand<OpenFile> closeFileCommand, bool isReadonly)
    {
        this.filename = filename;
        this.path = path;
        textDocument = new TextDocument();
        CloseFileCommand = closeFileCommand;
        IsReadonly = isReadonly;
    }
} 
