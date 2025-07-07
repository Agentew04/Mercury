using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Shapes;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Models.Compilation;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using SAAE.Engine;

namespace SAAE.Editor.ViewModels.Code;

public sealed partial class ProblemsViewModel : BaseViewModel
{   
    private readonly ILogger<ProblemsViewModel> logger = App.Services.GetRequiredService<ILogger<ProblemsViewModel>>();
    
    public ProblemsViewModel()
    {
        WeakReferenceMessenger.Default.Register<CompilationFinishedMessage>(this, OnCompilationFinished);
    }

    [ObservableProperty] 
    private string output = "";
    
    [ObservableProperty]
    private int selectedIndex = -1;

    [ObservableProperty] 
    private ObservableCollection<Diagnostic> diagnostics = [];

    private void OnCompilationFinished(object sender, CompilationFinishedMessage message)
    {
        CompilationResult result = message.Value;
        Diagnostics.Clear();
        if (result.Diagnostics is not null)
        {
            Diagnostics.AddRange(result.Diagnostics);
        }
    }

    partial void OnSelectedIndexChanged(int value)
    {
        Diagnostic diag = Diagnostics[value];
        logger.LogInformation("Opening file from problems view: {FilePath} at line {Line}, column {Column}", diag.FilePath, diag.Line, diag.Column);
        WeakReferenceMessenger.Default.Send(new FileOpenMessage
        {
            LineNumber = diag.Line,
            ColumnNumber = diag.Column,
            Path = diag.FilePath,
        });
    }
}