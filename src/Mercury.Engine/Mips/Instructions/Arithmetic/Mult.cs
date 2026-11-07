using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(15,0,24)]
[AssemblyFormat("mult ${Rs:reg}, ${Rt:reg}")]
public partial class Mult : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(20,16)]
    public byte Rt { get; set; }
}
