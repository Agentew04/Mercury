using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,17)] // opcode
[FormatExact<Instruction>(25,21,6)] // rs
[FormatExact<Instruction>(10,6,0)] // shift
[FormatExact<Instruction>(5,0,9)] // funct
public class Ctc1 : TypeFInstruction
{
    public byte Rt { get; private set; }
    public byte Fs { get; private set; }

    public override string ToString() => $"ctc1 ${Instruction.TranslateRegisterName(Rt)}, ${TranslateRegisterName(Fs)}" + FormatTrivia();

    public override void FromInt(int instruction)
    {
        Rt = (byte)((instruction >> 16) & 0b11111);
        Fs = (byte)((instruction >> 11) & 0b11111);
    }

    public override int ConvertToInt()
    {
        return OpCode << 26 // opcode
               | 0b00110 << 21 // ct
               | (Rt & 0b11111) << 16 // ft
               | (Fs & 0b11111) << 11; // fs
    }
}