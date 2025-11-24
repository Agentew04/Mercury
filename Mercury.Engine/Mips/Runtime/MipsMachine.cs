using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;
using Mercury.Engine.Common;
using Mercury.Engine.Mips.Runtime.OS;

namespace Mercury.Engine.Mips.Runtime;


public sealed class MipsMachine : Common.Machine {

    public MipsMachine(IMipsCpu cpu, MipsOperatingSystem os) {
        Cpu = cpu;
        Os = os;
    }
    
    public override IMipsCpu Cpu { get;  }
    
    public override MipsOperatingSystem  Os { get;  }
    
    public override RegisterCollection Registers => Cpu.Registers;
    
    public override void LoadElf(ELF<uint> elf) {
        Section<uint>? textSection = elf.GetSection(".text");
        uint textStart = textSection!.LoadAddress;
        uint textLength = textSection.Size;
        Cpu.Registers[MipsGprRegisters.Pc] = (int)elf.EntryPoint;
        SymbolTable<uint>? symbolTable = elf.GetSections<SymbolTable<uint>>().First();
        Cpu.ProgramEnd = symbolTable?.Entries?.FirstOrDefault(x => x.Name == "__end")?.Value ?? textStart + textLength;
        SymbolEntry<uint>? gpSymbol = symbolTable?.Entries?.First(x => x.Name == "_gp");
        if (gpSymbol is not null)
        {
            Cpu.Registers[MipsGprRegisters.Gp] = (int)gpSymbol.Value;
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
        Cpu.ProgramEnd = lastText + 1;
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