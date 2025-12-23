using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using ELFSharp;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using Mercury.Editor.Models.Compilation;
using Mercury.Editor.Models.Messages;
using Mercury.Engine.Common;
using Mercury.Engine.Common.Builders;
using Mercury.Engine.Mips.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mercury.Editor.Extensions;

namespace Mercury.Editor.Services;

/// <summary>
/// Service responsible to enable controls and the application to interact
/// with the engine to execute code. 
/// </summary>
public sealed class ExecuteService : BaseService<ExecuteService>, IDisposable {
    private readonly ICompilerService compilerService =
        App.Services.GetRequiredKeyedService<ICompilerService>(Architecture.Mips);

    private MipsMachine? currentMachine;
    private ELF<uint>? currentElf;

    public void LoadProgram() {
        CompilationResult result = compilerService.LastCompilationResult;

        if (result.Id == Guid.Empty 
            || !result.IsSuccess 
            || (result.Diagnostics?.Exists(x => x.Type == DiagnosticType.Error) ?? false))
        {
            return;
        }

        currentMachine?.Dispose();
        currentElf?.Dispose();

        FileStream elfFs = File.OpenRead(result.OutputPath!);
        currentElf = ELFReader.Load<uint>(elfFs, false);

        // cria memoria
        MemoryBuilder memoryBuilder = new MemoryBuilder()
            .With4Gb()
            .WithVolatileStorage()
            .WithBlockCapacity(16)
            .WithBlockSize(4096)
            .WithEndianess(currentElf.Endianess == Endianess.BigEndian
                ? Engine.Memory.Endianess.BigEndian
                : Engine.Memory.Endianess.LittleEndian);

        // criar maquina
        currentMachine = new MachineBuilder()
            .WithInMemoryStdio()
            .WithMemory(memoryBuilder
                .Build())
            .WithMips()
            .WithMarsOs()
            .WithMipsMonocycle()
            .Build();

        currentMachine.LoadElf(currentElf);
        
        // load symbols
        SymbolTable<uint> symbol = (SymbolTable<uint>)currentElf.Sections.First(x => x.Type == SectionType.SymbolTable);
        // load metadata of program starts
        List<ObjectFile> objFiles = [];
        Section<uint>? metadataSection = currentElf.GetSection("metadata");
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
                ulong highRangeAddress = currentElf.Endianess == Endianess.BigEndian
                    ? BinaryPrimitives.ReadUInt64BigEndian(labelBuffer)
                    : BinaryPrimitives.ReadUInt64LittleEndian(labelBuffer);
                uint lowRangeAddress = (uint)highRangeAddress;
                // le indice do arquivo
                ms.ReadExactly(indexBuffer);
                int fileIndex = currentElf.Endianess == Endianess.BigEndian 
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
        Logger.LogInformation("ELF has {StartCount} files and {SymbolCount} symbols.",
            objFiles.Count,
            meta.Symbols.Count);
        
        elfFs.Close();

        // publica evento de carregamento do programa
        ProgramLoadMessage loadMsg = new()
        {
            MipsMachine = currentMachine,
            Elf = currentElf,
            Metadata = meta
        };
        Logger.LogInformation("Programa carregado com sucesso: {OutputPath}", result.OutputPath);
        WeakReferenceMessenger.Default.Send(loadMsg);
    }

    public MipsMachine GetCurrentMachine()
    {
        return currentMachine!;
    }

    public void Dispose()
    {
        currentMachine?.Dispose();
    }
}
