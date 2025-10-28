using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,17)] // opcode
[FormatExact<Instruction>(25,21,8)] // rs
[FormatExact<Instruction>(17,17,0)] // nd
[FormatExact<Instruction>(16,16,1)] // tf
public class Bc1t : TypeFInstruction
{
    public byte Cc { get; private set; }
    public short Offset { get; private set; }

    public override string ToString() => $"bc1f {Offset<<2:X4}" + FormatTrivia();

    public override void FromInt(int instruction)
    {
        Offset = (short)(instruction&0xFFFF);
        Cc = (byte)((instruction>>18) & 0b111);
    }

    public override int ConvertToInt()
    {
        return OpCode << 26 // opcode
               | 0b01000 << 21 // bc
               | (Cc & 0b1111) << 16 // cc
               | (1 << 16) // set the 'tf' bit
               | Offset & 0xFFFF; // offset
    }
}