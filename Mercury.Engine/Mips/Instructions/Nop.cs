using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,0,0)]
public partial class Nop : IInstruction {
    public override string ToString() => "nop";
}
