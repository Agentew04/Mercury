namespace SAAE.Engine.Mips.Instructions;

public class Cvt_D : TypeFInstruction
{
    public byte Fmt { get; private set; }
    public byte Fs { get; private set; }
    public byte Fd { get; private set; }

    public bool IsDouble => Fmt == DoublePrecisionFormat;
    
    public override string ToString() => $"cvt.d.{FormatFmt(Fmt)} ${TranslateRegisterName(Fd)}, ${TranslateRegisterName(Fs)}" + FormatTrivia();
    
    public override void FromInt(int instruction)
    {
        Fmt = (byte)((instruction >> 21) & 0b11111);
        Fs = (byte)((instruction >> 11) & 0b11111);
        Fd = (byte)((instruction >> 6) & 0b11111);
    }

    public override int ConvertToInt()
    {
        return OpCode << 26 // opcode
               | (Fmt & 0b11) << 21 // fmt
               | (Fs & 0b11111) << 11 // fs
               | (Fd & 0b11111) << 6 // fd
               | 0b100001; // cvt.d
    }
}