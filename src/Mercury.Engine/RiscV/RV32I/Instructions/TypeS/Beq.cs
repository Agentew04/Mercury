using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.RiscV.RV32I.Instructions.TypeS;

[Instruction]
[FormatExact(14,12,0)]
[FormatExact(6,2,0b11000)]
[FormatExact(1,0,0b11)]
public partial class Beq : IInstruction
{
    [Field(19,15)]
    public byte Rs1 { get; set; }
    
    [Field(24,20)]
    public byte Rs2 { get; set; }
    
    [Field(7,7)]
    private byte Offset1 { get; set; }
    
    [Field(11,8)]
    private byte Offset2 { get; set; }
    
    [Field(30,25)]
    private byte Offset3 { get; set; }
    
    [Field(31,31)]
    private byte Offset4 { get; set; }

    public short Imm
    {
        get => (short)(Offset1 << 11 | Offset2 << 1 | Offset3 << 5 | Offset4 << 12);
        set
        {
            Offset1 = (byte)((value >> 11) & 0b1);
            Offset2 = (byte)((value >> 1) & 0b1111);
            Offset3 = (byte)((value >> 5) & 0b111111);
            Offset4 = (byte)((value >> 12) & 0b1);
        }
    }
    
}