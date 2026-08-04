using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.RiscV.RV32I.Instructions;

[Instruction]
[FormatExact(31,28,0)]
[FormatExact(19,0,0b00000000000000001111)]
public partial class Fence : IInstruction
{
    [Field(27,24)]
    public byte Pred { get; set; }
    
    [Field(23,20)]
    public byte Succ { get; set; }
}