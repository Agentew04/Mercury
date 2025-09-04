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
using SAAE.Engine.Common;

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
    
    private static void Started(object sender, CompilationStartedMessage msg)
    {
        (sender as StatusBar)!.StatusText.Text = StatusBarResources.CompilationStartedValue;
    }
    
    private static void Finished(object sender, CompilationFinishedMessage msg) {
        StatusBar bar = (StatusBar)sender;
        if (bar.compilerService.Value.LastCompilationResult.IsSuccess)
        {
            bar.StatusText.Text = StatusBarResources.CompilationEndedValue;
        }
        else
        {
            bar.StatusText.Text = string.Format(StatusBarResources.CompilationEndedFailureValue, 
                bar.compilerService.Value.LastCompilationResult.Diagnostics?.Count(x => x.Type == DiagnosticType.Error));
        }
    }

    public void Dispose() {
        LocalizationManager.CultureChanged -= Localize;
    }
}