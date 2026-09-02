using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.RiscV.RV32I.Instructions;

[Instruction]
[FormatExact(14,12,0b011)]
[FormatExact(6,2,0b00100)]
[FormatExact(1,0,0b11)]
public partial class Sltiu : IInstruction
{
    [Field(11,7)]
    public byte Byte { get; set; }
    
    [Field(19,15)]
    public byte Rs1 { get; set; }
    
    [Signed]
    [Field(31,20)]
    public short Imm { get; set; }
}