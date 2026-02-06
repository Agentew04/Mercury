using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

/// <summary>
/// The unconditional jump instruction
/// </summary>
[Instruction]
[FormatExact(31,26,2)]
public partial class J : IInstruction {
    
    [Field(25,0)]
    public int Immediate { get; set; }
    
    public string ToString(byte highOrderPc) => $"j 0x{(highOrderPc << 26) | (Immediate << 2):X7}";
}
