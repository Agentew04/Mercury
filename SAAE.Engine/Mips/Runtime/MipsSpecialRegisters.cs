using SAAE.Engine.Common;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Runtime;

[RegisterBankDefinition(Architecture.Mips, Processor = 2)]
public enum MipsSpecialRegisters {
    [Register(8, "vaddr", 32, false)]
    Vaddr,
    [Register(12, "status",32,false)]
    Status,
    [Register(13, "cause",32,false)]
    Cause,
    [Register(14, "epc",32,false)]
    Epc
}