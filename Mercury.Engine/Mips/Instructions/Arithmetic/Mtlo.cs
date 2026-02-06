using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(20,0,19)]
public partial class Mtlo : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }

    public override string ToString() => $"mtlo ${Instruction.TranslateRegisterName(Rs)}";
}
