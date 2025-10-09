using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,28,14)] // opcode 4 bits
public class Swcz : TypeIInstruction
{
    public byte Base
    {
        get => Rs;
        set => Rs = value;
    }
    
    public byte Coprocessor { get; set; }

    public Swcz()
    {
        OpCode = 0b1110;
    }
    
    public override Regex GetRegularExpression()
    {
        throw new NotSupportedException();
    }

    public override string ToString() => $"swc{Coprocessor} ${TranslateRegisterName(Rt)}, {Immediate:X4}(${TypeFInstruction.TranslateRegisterName(Base)})" + FormatTrivia();
    
    public override int ConvertToInt()
    {
        return OpCode << 28 
               | ((Coprocessor & 0b11) << 26)
               | (Base & 0b11111) << 21
               | (Rt & 0b11111) << 16
               | (Immediate & 0xFFFF);
    }

    public override void FromInt(int instruction)
    {
        OpCode = (byte)(instruction >> 28 & 0b1111);
        Coprocessor = (byte)(instruction >> 26 & 0b11);
        Rs = (byte)(instruction >> 21 & 0b11111);
        Rt = (byte)(instruction >> 16 & 0b11111);
        Immediate = (short)(instruction & 0xFFFF);
    }
}