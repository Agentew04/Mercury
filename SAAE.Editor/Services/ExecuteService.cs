using System;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using ELFSharp.ELF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Models.Compilation;
using SAAE.Editor.Models.Messages;
using SAAE.Engine;
using SAAE.Engine.Mips.Runtime;
using Machine = SAAE.Engine.Mips.Runtime.Machine;

namespace SAAE.Editor.Services;

/// <summary>
/// Service responsible to enable controls and the application to interact
/// with the engine to execute code. 
/// </summary>
public class ExecuteService
{
    private Machine? currentMachine;
    private readonly ICompilerService compilerService = App.Services.GetRequiredKeyedService<ICompilerService>(Architecture.Mips);
    private readonly ILogger<ExecuteService> logger = App.Services.GetRequiredService<ILogger<ExecuteService>>();

    public ExecuteService()
    {
        WeakReferenceMessenger.Default.Register<CompilationFinishedMessage>(this, OnCompile);
    }

    private void OnCompile(object recipient, CompilationFinishedMessage message)
    {
        if (message.Value == Guid.Empty)
        {
            // mensagem vazia
            return;
        }

        CompilationResult result = compilerService.LastCompilationResult;
        if (result.Id != message.Value)
        {
            // compilou varias vezes?
            return;
        }

        if (!result.IsSuccess || (result.Diagnostics?.Exists(x => x.Type == DiagnosticType.Warning) ?? false))
        {
            // a compilacao falhou ou tem avisos
            return;
        }

        currentMachine?.Dispose();

        // criar maquina
        Machine m = new MachineBuilder()
            .With4GbRam()
            .WithInMemoryStdio()
            .WithMarsOs()
            .WithMipsMonocycle()
            .Build();
        currentMachine = m;
        
        ELF<uint> elf = ELFReader.Load<uint>(result.OutputPath);
        m.LoadElf(elf);

        // publica evento de carregamento do programa
        ProgramLoadMessage loadMsg = new()
        {
            Machine = m
        };
        logger.LogInformation("Programa carregado com sucesso: {ProgramPath}", result.OutputPath);
        WeakReferenceMessenger.Default.Send(loadMsg);
    }
    
    public Machine GetCurrentMachine()
    {
        return currentMachine!;
    }
}
