namespace SAAE.Engine.Mips.Instructions;

public class C : TypeFInstruction
{
    public byte Fmt { get; private set; }
    public byte Ft { get; private set; }
    public byte Fs { get; private set; }
    public byte Cc { get; private set; }
    public byte Cond { get; private set; }

    public bool IsDouble => Fmt == DoublePrecisionFormat;
    
    public override string ToString()
    {
        return $"C.{FormatCond(Cond)}.{FormatFmt(Fmt)} {(Cc!=0?$"{Cc}, ":"")}${TranslateRegisterName(Fs)}, ${TranslateRegisterName(Ft)}" + FormatTrivia();
    }

    private static string FormatCond(byte value)
    {
        return value switch
        {
            0b0000 => "f",
            0b0001 => "un",
            0b0010 => "eq",
            0b0011 => "ueq",
            0b0100 => "olt",
            0b0101 => "ult",
            0b0110 => "ole",
            0b0111 => "ule",
            0b1000 => "sf",
            0b1001 => "ngle",
            0b1010 => "seq",
            0b1011 => "ngl",
            0b1100 => "lt",
            0b1101 => "nge",
            0b1110 => "le",
            0b1111 => "ngt",
            _ => "unknown"
        };
    }

    public override void FromInt(int instruction)
    {
        Fmt = (byte)((instruction >> 21) & 0b11111);
        Ft = (byte)((instruction >> 16) & 0b11111);
        Fs = (byte)((instruction >> 11) & 0b11111);
        Cc = (byte)((instruction >> 8) & 0b111);
        Cond = (byte)(instruction & 0b1111);
    }

    public override int ConvertToInt()
    {
        return OpCode << 26 // opcode
               | (Fmt & 0b11111) << 21 // fmt
               | (Ft & 0b11111) << 16 // ft
               | (Fs & 0b11111) << 11 // fs
               | (Cc & 0b111) << 8 // cc
               | 0b11 << 4 // FC
               | (Cond & 0b1111); // cond
    }
}
