using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

/// <summary>
/// Move from HI. Rd = HI. Format: R, funct 0x10.
/// </summary>
[Instruction]
[FormatExact(31,16,0)]
[FormatExact(10,6,0)]
[FormatExact(5,0,16)]
public partial class Mfhi : IInstruction {

    [Field(15,11)]
    public byte Rd { get; set; }

    public override string ToString() => $"mfhi ${Instruction.TranslateRegisterName(Rd)}";
}
