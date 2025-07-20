using System;
using CommunityToolkit.Mvvm.Messaging;
using ELFSharp.ELF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Models.Compilation;
using SAAE.Editor.Models.Messages;
using SAAE.Engine;
using SAAE.Engine.Common.Builders;
using Machine = SAAE.Engine.Mips.Runtime.Machine;

namespace SAAE.Editor.Services;

/// <summary>
/// Service responsible to enable controls and the application to interact
/// with the engine to execute code. 
/// </summary>
public sealed class ExecuteService : BaseService<ExecuteService>, IDisposable
{
    private Machine? currentMachine;

    public ExecuteService()
    {
        Logger.LogInformation("Initializing ExecutionService");
        WeakReferenceMessenger.Default.Register<CompilationFinishedMessage>(this, OnCompile);
    }

    private static void OnCompile(object recipient, CompilationFinishedMessage message) {
        ExecuteService service = (ExecuteService)recipient;
        if (message.Value.Id == Guid.Empty 
            || !message.Value.IsSuccess 
            || (message.Value.Diagnostics?.Exists(x => x.Type == DiagnosticType.Error) ?? false))
        {
            return;
        }

        service.currentMachine?.Dispose();

        // criar maquina
        service.currentMachine = new MachineBuilder()
            .WithInMemoryStdio()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithVolatileStorage()
                .WithPageCapacity(16)
                .WithPageSize(4096)
                .WithLittleEndian()
                .Build())
            .WithMips()
            .WithMarsOs()
            .WithMipsMonocycle()
            .Build();

        ELF<uint> elf = ELFReader.Load<uint>(message.Value.OutputPath);
        service.currentMachine.LoadElf(elf);

        // publica evento de carregamento do programa
        ProgramLoadMessage loadMsg = new()
        {
            Machine = service.currentMachine
        };
        service.Logger.LogInformation("Programa carregado com sucesso: {OutputPath}", message.Value.OutputPath);
        WeakReferenceMessenger.Default.Send(loadMsg);
    }
    
    public Machine GetCurrentMachine()
    {
        return currentMachine!;
    }

    public void Dispose()
    {
        currentMachine?.Dispose();
    }
}
