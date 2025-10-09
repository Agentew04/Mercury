using SAAE.Engine.Common;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Runtime;

[RegisterInfoDefinition(Architecture.Mips)]
public enum MipsFpuControlRegisters
{
    [RegisterInfo(0, "FIR",32)]
    Fir,
    [RegisterInfo(1, "FEXR",32)]
    Fexr,
    [RegisterInfo(2, "FENR",32)]
    Fenr,
    [RegisterInfo(3, "FCSR",32)]
    Fcsr,
    [RegisterInfo(4, "FCCR",32)]
    Fccr,
}