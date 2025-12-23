using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Editor.Models.Compilation;
using Mercury.Editor.Models.Messages;
using Mercury.Editor.Views.CodeView;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.ViewModels.Code;

public sealed partial class ProblemsViewModel : BaseViewModel<ProblemsViewModel, ProblemsView> {
    
    public ProblemsViewModel()
    {
        WeakReferenceMessenger.Default.Register<CompilationFinishedMessage>(this, OnCompilationFinished);
    }

    [ObservableProperty] 
    private string output = "";
    
    [ObservableProperty] 
    private ObservableCollection<Diagnostic> diagnostics = [];

    private static void OnCompilationFinished(object sender, CompilationFinishedMessage message) {
        
        ProblemsViewModel vm = (ProblemsViewModel)sender;
        CompilationResult result = message.Value;
        vm.Diagnostics.Clear();
        if (result.Diagnostics is not null)
        {
            vm.Diagnostics.AddRange(result.Diagnostics);
        }
    }

    public void OnSelectedIndexChanged(int value)
    {
        if(value == -1)
        {
            Logger.LogInformation("Problem null. Skipping.");
            return;   
        }
        Diagnostic diag = Diagnostics[value];
        Logger.LogInformation("Opening file from problems view: {FilePath} at line {Line}, column {Column}", diag.FilePath, diag.Line, diag.Column);
        WeakReferenceMessenger.Default.Send(new FileOpenMessage
        {
            LineNumber = diag.Line,
            ColumnNumber = diag.Column,
            Path = diag.FilePath,
        });
    }
}