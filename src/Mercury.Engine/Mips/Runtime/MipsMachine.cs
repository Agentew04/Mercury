using ELFSharp.ELF;
using ELFSharp.ELF.Sections;

namespace Mercury.Engine.Mips.Runtime;

public sealed class MipsMachine : Common.Machine {
    public override void LoadElf(ELF<uint> elf) {
        base.LoadElf(elf);

        // set information directly to registers
        CpuModule.Registers.Set(MipsGprRegisters.Pc, (int)elf.EntryPoint);
        SymbolTable<uint>? symbolTable = elf.GetSections<SymbolTable<uint>>().First();
        SymbolEntry<uint>? gpSymbol = symbolTable?.Entries?.First(x => x.Name == "_gp");
        if (gpSymbol is not null) {
            CpuModule.Registers.Set(MipsGprRegisters.Gp, (int)gpSymbol.Value);
        }
    }

    private const uint TextSegmentAddress = 0x0040_0000;
    private const uint DataSegmentAddress = 0x1001_0000;
}