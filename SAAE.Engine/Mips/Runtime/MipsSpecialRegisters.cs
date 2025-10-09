using SAAE.Engine.Common;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Runtime;

[RegisterInfoDefinition(Architecture.Mips)]
public enum MipsSpecialRegisters {
    [RegisterInfo(8, "vaddr", 32)]
    Vaddr,
    [RegisterInfo(12, "status",32 )]
    Status,
    [RegisterInfo(13, "cause",32)]
    Cause,
    [RegisterInfo(14, "epc",32)]
    Epc
}