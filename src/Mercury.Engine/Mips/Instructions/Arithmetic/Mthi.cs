using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(20,0,17)]
[AssemblyFormat("mthi ${Rs:reg}")]
public partial class Mthi : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
}
