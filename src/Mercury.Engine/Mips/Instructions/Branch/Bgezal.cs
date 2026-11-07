using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,1)]
[FormatExact(20,16,17)]
[AssemblyFormat("bgezal ${Rs:reg}, 0x{Immediate:X4}")]
public partial class Bgezal : IInstruction{

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(15,0)]
    public short Immediate { get; set; }
}
