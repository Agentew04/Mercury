using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

/// <summary>
/// Jump and link. Jumps to the target address and stores the return address in $ra.
/// Format: J, opcode 0x03.
/// </summary>
[Instruction]
[FormatExact(31,26,3)]
public partial class Jal : IInstruction {

    [Field(25,0)]
    public int Immediate { get; set; }

    public override string ToString() => ToString(0);

    public string ToString(byte highOrderPc) => $"jal 0x{(highOrderPc << 26) | (Immediate << 2):X7}";
}
