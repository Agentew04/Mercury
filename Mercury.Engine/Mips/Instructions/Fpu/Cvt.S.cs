namespace Mercury.Engine.Mips.Instructions;

public class Cvt_S : TypeFInstruction
{
    public byte Fmt { get; private set; }
    public byte Fs { get; private set; }
    public byte Fd { get; private set; }

    public bool IsDouble => Fmt == DoublePrecisionFormat;
    
    public override string ToString() => $"cvt.s.{FormatFmt(Fmt)} ${TranslateRegisterName(Fd)}, ${TranslateRegisterName(Fs)}" + FormatTrivia();

    public override void FromInt(int instruction)
    {
        Fmt = (byte)((instruction >> 21) & 0b11111);
        Fs = (byte)((instruction >> 11) & 0b11111);
        Fd = (byte)((instruction >> 6) & 0b11111);
    }

    public override int ConvertToInt()
    {
        return OpCode << 26
            | Fmt << 21
            | (Fs & 0b11111) << 11
            | (Fd & 0b11111) << 6
            | 0b100000; // cvt.s
    }
}