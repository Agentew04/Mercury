using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.RiscV.RV32I.Instructions;

[Instruction]
[FormatExact(14,12,0b010)]
[FormatExact(6,2,0b01000)]
[FormatExact(1,0,0b11)]
public partial class Sw : IInstruction
{
    [Field(19,15)]
    public byte Rs1 { get; set; }
    
    [Field(24,20)]
    public byte Rs2 { get; set; }
    
    [Field(11,7)]
    private byte Offset1 { get; set; }
    
    [Field(31,25)]
    private byte Offset2 { get; set; }

    public short Imm
    {
        get => (short)((Offset1 & 0b11111) | Offset2 << 5);
        set
        {
            Offset1 = (byte)(value & 0b11111);
            Offset2 = (byte)((value >> 5) & 0b1111111);
        }
    }
}