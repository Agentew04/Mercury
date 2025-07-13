using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using System.Linq;
using SAAE.Editor.Localization;
using SAAE.Editor.Models.Compilation;
using SAAE.Engine;

namespace SAAE.Editor.Views;

public partial class StatusBar : UserControl, IDisposable
{
    private readonly Lazy<ICompilerService> compilerService = new(
        () => App.Services.GetRequiredKeyedService<ICompilerService>(Architecture.Mips));
    
    public StatusBar()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<CompilationStartedMessage>(this, Started);
        WeakReferenceMessenger.Default.Register<CompilationFinishedMessage>(this, Finished);
        StatusText.Text = StatusBarResources.ReadyValue;
        LocalizationManager.CultureChanged += Localize;
    }
    
    // TODO: adicionar:
    // linha e coluna atual
    // numero de erros, total e nesse arquivo: 5 errors in 2 files (3 here)

    private void Localize(CultureInfo culture) {
        // reseta a localizacao para evitar ficar com a lingua velha
        // quando troca
        StatusText.Text = StatusBarResources.ReadyValue;
    }
    
    private void Started(object sender, CompilationStartedMessage msg)
    {
        StatusText.Text = StatusBarResources.CompilationStartedValue;
    }
    
    private void Finished(object sender, CompilationFinishedMessage msg)
    {
        if (compilerService.Value.LastCompilationResult.IsSuccess)
        {
            StatusText.Text = StatusBarResources.CompilationEndedValue;
        }
        else
        {
            StatusText.Text = string.Format(StatusBarResources.CompilationEndedFailureValue, 
                compilerService.Value.LastCompilationResult.Diagnostics?.Count(x => x.Type == DiagnosticType.Error));
        }
    }

    public void Dispose() {
        LocalizationManager.CultureChanged -= Localize;
    }
}