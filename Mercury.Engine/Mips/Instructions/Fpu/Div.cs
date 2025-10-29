using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,17)] // opcode
[FormatExact<Instruction>(25,21,[16,17,20])] // rs
[FormatExact<Instruction>(5,0,3)] // funct
public class Div_float : TypeFInstruction
{
    public byte Fmt { get; private set; }
    public byte Ft { get; private set; }
    public byte Fs { get; private set; }
    public byte Fd { get; private set; }

    public bool IsDouble => Fmt == DoublePrecisionFormat;

    public override string ToString() => $"div.{FormatFmt(Fmt)} ${TranslateRegisterName(Fd)}, ${TranslateRegisterName(Fs)}, ${TranslateRegisterName(Ft)}" + FormatTrivia();

    public override void FromInt(int instruction)
    {
        Fmt = (byte)((instruction >> 21) & 0b11111);
        Ft = (byte)((instruction >> 16) & 0b11111);
        Fs = (byte)((instruction >> 11) & 0b11111);
        Fd = (byte)((instruction >> 6) & 0b11111);
    }

    public override int ConvertToInt()
    {
        return OpCode << 26 // opcode
               | (Fmt & 0b11111) << 21 // fmt
               | (Ft & 0b11111) << 16 // ft
               | (Fs & 0b11111) << 11 // fs
               | (Fd & 0b11111) << 6 // fd
               | 0b000011; // div
    }
}