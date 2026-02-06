using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,16,0)]
[FormatExact(10,6,0)]
[FormatExact(5,0,18)]
public partial class Mflo : IInstruction {

    [Field(15,11)]
    public byte Rd { get; set; }

    public override string ToString() => $"mflo ${Instruction.TranslateRegisterName(Rd)}";
}
