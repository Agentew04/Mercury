using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

/// <summary>
/// Load upper immediate. Rt = Immediate << 16. Format: I, opcode 0x0F, rs = 0.
/// </summary>
[Instruction]
[FormatExact(31,26,15)] // opcode
[FormatExact(25,21,0)]
public partial class Lui : IInstruction {

    [Field(20,16)]
    public byte Rt { get; set; }

    [Field(15,0)]
    public short Immediate { get; set; }

    public override string ToString() => $"lui ${Instruction.TranslateRegisterName(Rt)}, {(ushort)Immediate}";
}
