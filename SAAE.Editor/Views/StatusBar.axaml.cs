using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using System.Linq;
using SAAE.Editor.Models.Compilation;
using SAAE.Engine;

namespace SAAE.Editor.Views;

public partial class StatusBar : UserControl
{
    private readonly Lazy<ICompilerService> compilerService = new(
        () => App.Services.GetRequiredKeyedService<ICompilerService>(Architecture.Mips));
    
    public StatusBar()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<CompilationStartedMessage>(this, Started);
        WeakReferenceMessenger.Default.Register<CompilationFinishedMessage>(this, Finished);
        StatusText.Text = Localization.StatusBarResources.ReadyValue;
    }
    
    // TODO: adicionar:
    // linha e coluna atual
    // numero de erros, total e nesse arquivo: 5 errors in 2 files (3 here)

    private void Started(object sender, CompilationStartedMessage msg)
    {
        StatusText.Text = Localization.StatusBarResources.CompilationStartedValue;
    }
    
    private void Finished(object sender, CompilationFinishedMessage msg)
    {
        if (compilerService.Value.LastCompilationResult.IsSuccess)
        {
            StatusText.Text = Localization.StatusBarResources.CompilationEndedValue;
        }
        else
        {
            StatusText.Text = string.Format(Localization.StatusBarResources.CompilationEndedFailureValue, 
                compilerService.Value.LastCompilationResult.Diagnostics?.Count(x => x.Type == DiagnosticType.Error));
        }
    }
}