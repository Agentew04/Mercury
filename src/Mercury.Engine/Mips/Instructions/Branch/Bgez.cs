using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,1)]
[FormatExact(20,16,1)]
[AssemblyFormat("bgez ${Rs:reg}, 0x{Offset:X4}")]
public partial class Bgez : IInstruction{

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(15,0)]
    public short Offset { get; set; }
}
