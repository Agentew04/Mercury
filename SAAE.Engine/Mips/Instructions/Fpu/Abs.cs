using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,17)] // opcode
[FormatExact<Instruction>(20,16,0)] // rt
[FormatExact<Instruction>(25,21,[16,17,20])] // rs
[FormatExact<Instruction>(5,0,5)] // funct
public class Abs : TypeFInstruction
{
    private byte fmt = SinglePrecisionFormat;
    public byte Fs { get; set; }
    public byte Fd { get; set; }

    private const byte Function = 0b000101;
    
    public bool IsDouble => fmt == DoublePrecisionFormat;
    
    public override string ToString() => $"abs.{FormatFmt(fmt)} ${TranslateRegisterName(Fd)}, ${TranslateRegisterName(Fs)}" + FormatTrivia();

    public override void FromInt(int instruction)
    {
        fmt = (byte)((instruction >> 21) & 0b11111);
        Fs = (byte)((instruction >> 16) & 0b11111);
        Fd = (byte)((instruction >> 11) & 0b11111);
    }

    public override int ConvertToInt()
    {
        const byte five = 0b11111;
        return OpCode << 26 // opcode 
               | (fmt & five) << 21 // fmt
               | (Fs & five) << 16 // fs
               | (Fd & five) << 11 // fd
               | (Function & 0b111111); // function
    }
}
