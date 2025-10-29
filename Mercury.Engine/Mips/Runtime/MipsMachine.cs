using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;

namespace Mercury.Engine.Mips.Runtime;

/// <summary>
/// Extends the functionality of <see cref="Machine"/> to a <see cref="Mercury.Engine.Common.Architecture.Mips"/> machine.
/// </summary>
public sealed class MipsMachine : Mercury.Engine.Common.Machine {

    public new required IMipsCpu Cpu { get; init; }
    
    public override void LoadElf(ELF<uint> elf) {
        Section<uint>? textSection = elf.GetSection(".text");
        uint textStart = textSection!.LoadAddress;
        uint textLength = textSection.Size;
        Cpu.RegisterBank[MipsGprRegisters.Pc] = (int)elf.EntryPoint;
        SymbolTable<uint>? symbolTable = elf.GetSections<SymbolTable<uint>>().First();
        Cpu.DropoffAddress = symbolTable?.Entries?.First(x => x.Name == "__end")?.Value ?? textStart + textLength;
        SymbolEntry<uint>? gpSymbol = symbolTable?.Entries?.First(x => x.Name == "_gp");
        if (gpSymbol is not null)
        {
            Registers[MipsGprRegisters.Gp] = (int)gpSymbol.Value;
        }

        // use segments to load data into memory
        foreach (Segment<uint>? segment in elf.Segments) {
            if (segment.Type != SegmentType.Load) {
                continue;
            }
            DataMemory.Write(segment.Address, segment.GetMemoryContents());
        }
    }
    
    private const uint TextSegmentAddress = 0x0040_0000;
    private const uint DataSegmentAddress = 0x1001_0000;
    
    public override void LoadProgram(Span<int> text, Span<int> data) {
        uint lastText = Load(text, TextSegmentAddress);
        Cpu.DropoffAddress = lastText + 1;
        _ = Load(data, DataSegmentAddress);
    }
}

public class MemoryAccessEventArgs : EventArgs {
    public required ulong Address { get; init; }
    public required int Size { get; init; }
    public required MemoryAccessMode Mode { get; init; }
    public required MemoryAccessSource Source { get; init; }
}

public enum MemoryAccessMode {
    Read,
    Write
}

public enum MemoryAccessSource {
    Code,
    Instruction
}