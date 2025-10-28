using Mercury.Engine.Common;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Runtime;

[RegisterInfoDefinition(Architecture.Mips)]
public enum MipsSpecialRegisters {
    [RegisterInfo(8, "vaddr")]
    Vaddr,
    [RegisterInfo(12, "status")]
    Status,
    [RegisterInfo(13, "cause")]
    Cause,
    [RegisterInfo(14, "epc")]
    Epc
}