using SAAE.Engine.Common;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Runtime;

[RegisterBankDefinition(Architecture.Mips, Processor = 1)]
public enum MipsFpuControlRegisters
{
    [Register(0, "FIR",32, false)]
    Fir,
    [Register(1, "FEXR",32,false)]
    Fexr,
    [Register(2, "FENR",32,false)]
    Fenr,
    [Register(3, "FCSR",32,false)]
    Fcsr,
    [Register(4, "FCCR",32,false)]
    Fccr,
}