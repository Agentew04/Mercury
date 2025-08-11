namespace SAAE.Engine.Mips.Instructions;

public class Sub_float : TypeFInstruction
{
    public byte Fmt { get; private set; }
    public byte Ft { get; private set; }
    public byte Fs { get; private set; }
    public byte Fd { get; private set; }
    
    public override string ToString() => $"sub.{FormatFmt(Fmt)} ${TranslateRegisterName(Fd)}, ${TranslateRegisterName(Fs)}, ${TranslateRegisterName(Ft)}" + FormatTrivia();

    public override void FromInt(int instruction)
    {
        throw new NotImplementedException();
    }

    public override int ConvertToInt()
    {
        return OpCode << 26 // opcode
               | (Fmt & 0b11111) << 21 // fmt
               | (Ft & 0b11111) << 16 // ft
               | (Fs & 0b11111) << 11 // fs
               | (Fd & 0b11111) << 6 // fd
               | 0b000001; // sub
    }
}