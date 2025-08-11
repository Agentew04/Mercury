using SAAE.Generators;

namespace SAAE.Engine.Mips.Runtime;

[RegisterInfoDefinition]
public enum MipsFpuControlRegisters
{
    [RegisterInfo(0, "FIR")]
    Fir,
    [RegisterInfo(1, "FEXR")]
    Fexr,
    [RegisterInfo(2, "FENR")]
    Fenr,
    [RegisterInfo(3, "FCSR")]
    Fcsr,
    [RegisterInfo(4, "FCCR")]
    Fccr,
}