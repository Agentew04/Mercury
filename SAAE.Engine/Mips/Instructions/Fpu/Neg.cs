using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,17)] // opcode
[FormatExact<Instruction>(20,16,0)] // rt
[FormatExact<Instruction>(25,21,[16,17,20])] // rs
[FormatExact<Instruction>(5,0,7)] // funct
public class Neg : TypeFInstruction
{
    public byte Fmt { get; private set; }
    public byte Fs { get; private set; }
    public byte Fd { get; private set; }
    
    public bool IsDouble => Fmt == DoublePrecisionFormat;
    
    public override string ToString() => $"neg.{FormatFmt(Fmt)} ${TranslateRegisterName(Fd)}, ${TranslateRegisterName(Fs)}" + FormatTrivia();

    public override void FromInt(int instruction)
    {
        Fmt = (byte)((instruction >> 21) & 0b11111);
        Fs = (byte)((instruction >> 11) & 0b11111);
        Fd = (byte)((instruction >> 6) & 0b11111);
    }

    public override int ConvertToInt()
    {
        return OpCode << 26 // opcode
               | (Fmt & 0b11111) << 21 // fmt
               | (Fs & 0b11111) << 11 // fs
               | (Fd & 0b11111) << 6 // fd
               | 0b000111; // neg
    }
}