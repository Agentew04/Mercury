using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,16,0)]
[FormatExact(10,6,0)]
[FormatExact(5,0,16)]
[AssemblyFormat("mfhi ${Rd:reg}")]
public partial class Mfhi : IInstruction {

    [Field(15,11)]
    public byte Rd { get; set; }
}
