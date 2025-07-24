using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using ELFSharp;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
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
    private ELF<uint>? currentElf;

    public ExecuteService()
    {
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
        service.currentElf?.Dispose();

        FileStream elfFs = File.OpenRead(message.Value.OutputPath!);
        service.currentElf = ELFReader.Load<uint>(elfFs, false);

        // cria memoria
        MemoryBuilder memoryBuilder = new MemoryBuilder()
            .With4Gb()
            .WithVolatileStorage()
            .WithPageCapacity(16)
            .WithPageSize(4096)
            .WithEndianess(service.currentElf.Endianess == Endianess.BigEndian
                ? Engine.Memory.Endianess.BigEndian
                : Engine.Memory.Endianess.LittleEndian);

        // criar maquina
        service.currentMachine = new MachineBuilder()
            .WithInMemoryStdio()
            .WithMemory(memoryBuilder
                .Build())
            .WithMips()
            .WithMarsOs()
            .WithMipsMonocycle()
            .Build();

        service.currentMachine.LoadElf(service.currentElf);
        
        // load symbols
        SymbolTable<uint> symbol = (SymbolTable<uint>)service.currentElf.Sections.First(x => x.Type == SectionType.SymbolTable);
        // load metadata of program starts
        List<ObjectFile> objFiles = [];
        Section<uint>? metadataSection = service.currentElf.GetSection("metadata");
        byte[]? contents = metadataSection?.GetContents();
        if (contents is not null) {
            Span<byte> labelBuffer = stackalloc byte[8];
            Span<byte> indexBuffer = stackalloc byte[4];
            StringBuilder sb = new();
            using MemoryStream ms = new(contents);
            while (ms.Position < ms.Length) {
                // le nome do arquivo
                int value;
                while ((value = ms.ReadByte()) != 0 && value != -1) {
                    sb.Append((char)(byte)value);
                }
                // le endereco de inicio
                ms.ReadExactly(labelBuffer);
                ulong highRangeAddress = BitConverter.ToUInt64(labelBuffer);
                uint lowRangeAddress = (uint)highRangeAddress;
                // le indice do arquivo
                ms.ReadExactly(indexBuffer);
                int fileIndex = service.currentElf.Endianess == Endianess.BigEndian 
                    ? BinaryPrimitives.ReadInt32BigEndian(indexBuffer) 
                    : BinaryPrimitives.ReadInt32LittleEndian(indexBuffer);
                objFiles.Add(new ObjectFile(sb.ToString().ToFilePath(), lowRangeAddress, fileIndex));
                sb.Clear();
            }
        }
        ProgramMetadata meta = new() {
            Symbols = symbol.Entries.Select(x => new Symbol(x.Name, x.Value)).ToList(),
            Files = objFiles
        };
        service.Logger.LogInformation("ELF has {startCount} files and {symbolCount} symbols.",
            objFiles.Count,
            meta.Symbols.Count);
        
        elfFs.Close();

        // publica evento de carregamento do programa
        ProgramLoadMessage loadMsg = new()
        {
            Machine = service.currentMachine,
            Elf = service.currentElf,
            Metadata = meta
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
