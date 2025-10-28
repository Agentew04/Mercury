using Mercury.Engine.Common;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Runtime;

[RegisterInfoDefinition(Architecture.Mips)]
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