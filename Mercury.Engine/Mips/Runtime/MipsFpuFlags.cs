using Mercury.Engine.Common;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Runtime;

/// <summary>
/// A set of flags for the MIPS Floating Point Unit (FPU) aka Coprocessor 1.
/// </summary>
[ProcessorFlags(Architecture.Mips, Processor = 1)]
public enum MipsFpuFlags {
    N0,
    N1,
    N2,
    N3,
    N4,
    N5,
    N6,
    N7,
}