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

namespace SAAE.Editor.ViewModels.Code;

public sealed partial class ProblemsViewModel : BaseViewModel
{
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
        CompilationResult result = message.Value;
        Diagnostics.Clear();
        if (result.Diagnostics is not null)
        {
            Diagnostics.AddRange(result.Diagnostics);
        }
    }
}