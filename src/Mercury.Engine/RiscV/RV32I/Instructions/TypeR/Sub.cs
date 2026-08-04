using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.RiscV.RV32I.Instructions;

[Instruction]
[FormatExact(31,27,0b01000)]
[FormatExact(26,25,0)]
[FormatExact(14,12,0)]
[FormatExact(6,2,0b01100)]
[FormatExact(1,0,0b11)]
public partial class Sub : IInstruction
{
    [Field(11,7)]
    public byte Rd { get; set; }
    
    [Field(19,15)]
    public byte Rs1 { get; set; }
    
    [Field(24,20)]
    public byte Rs2 { get; set; }
}