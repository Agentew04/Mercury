using Mercury.Engine.Common;

namespace Mercury.Engine.RiscV.RV64I.Instructions;

/// <summary>
/// Base class that all RISC-V 64 instructions must inherit. 
/// </summary>
public abstract class Instruction {
    public abstract override string ToString();
    protected string Mnemonic => GetType().Name.ToLowerInvariant();
}