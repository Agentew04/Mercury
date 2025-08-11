using SAAE.Generators;

namespace SAAE.Engine.Mips.Runtime;

[RegisterInfoDefinition]
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