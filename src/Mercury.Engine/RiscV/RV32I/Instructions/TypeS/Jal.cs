using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.RiscV.RV32I.Instructions;

[Instruction]
[FormatExact(6,2,0b11011)]
[FormatExact(1,0,0b11)]
public partial class Jal : IInstruction
{
    [Field(11,7)]
    public byte Rd { get; set; }
    
    [Field(19,12)]
    private byte Offset1 { get; set; }
    
    [Field(20,20)]
    private byte Offset2 { get; set; }
    
    [Field(30,21)]
    private short Offset3 { get; set; }
    
    [Field(31,31)]
    private byte Offset4 { get; set; }

    public int Imm
    {
        get => Offset1 << 12 | Offset2 << 11 | Offset3 << 1 | Offset4 << 20;
        set
        {
            Offset1 = (byte)((value >> 12) & 0xFF);
            Offset2 = (byte)((value >> 11) & 0b1);
            Offset3 = (byte)((value >> 1) & 0b1111111111);
            Offset4 = (byte)((value >> 20) & 0b1);
        }
    }
}