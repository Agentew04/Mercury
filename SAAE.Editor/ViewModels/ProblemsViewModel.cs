using System;
using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models.Compilation;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using SAAE.Engine;

namespace SAAE.Editor.ViewModels;

public sealed partial class ProblemsViewModel : BaseViewModel
{
    private readonly ICompilerService compilerService = App.Services.GetRequiredKeyedService<ICompilerService>(Architecture.Mips);

    public ProblemsViewModel()
    {
        WeakReferenceMessenger.Default.Register<CompilationFinishedMessage>(this, OnCompilationFinished);
    }

    [ObservableProperty] 
    private string output = "";

    [ObservableProperty] 
    private ObservableCollection<Diagnostic> diagnostics = [];

    private void OnCompilationFinished(object sender, CompilationFinishedMessage message)
    {
        CompilationResult lastResult = compilerService.LastCompilationResult;
        if (lastResult.Id != message.Value)
        {
            Console.WriteLine("Recebi compilacao desatualizada");
            return;
        }
        
        Diagnostics.Clear();
        if (lastResult.Diagnostics is not null)
        {
            Diagnostics.AddRange(lastResult.Diagnostics);
        }
    }
}