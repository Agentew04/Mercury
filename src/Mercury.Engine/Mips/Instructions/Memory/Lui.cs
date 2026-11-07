using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,15)]
[FormatExact(25,21,0)]
[AssemblyFormat("lui ${Rt:reg}, {Immediate}")]
public partial class Lui : IInstruction {

    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,0)]
    public short Immediate { get; set; }
}
