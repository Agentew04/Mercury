using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(15,6,0)]
[FormatExact(5,0,26)]
[AssemblyFormat("div ${Rs:reg}, ${Rt:reg}")]
public partial class Div : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(20,16)]
    public byte Rt { get; set; }
}
