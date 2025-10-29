using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,17)] // opcode
[FormatExact<Instruction>(25,21,0)] // rs
[FormatExact<Instruction>(10,6,0)] // shift
[FormatExact<Instruction>(5,0,0)] // funct
public class Mfc1 : TypeFInstruction
{
    public byte Rt { get; private set; }
    public byte Fs { get; private set; }

    public override string ToString() => $"mfc1 ${Instruction.TranslateRegisterName(Rt)}, ${TranslateRegisterName(Fs)}" + FormatTrivia();

    public override void FromInt(int instruction)
    {
        Rt = (byte)(instruction >> 16 & 0b11111);
        Fs = (byte)(instruction >> 11 & 0b11111);
    }

    public override int ConvertToInt()
    {
        return OpCode << 26 // opcode
               | (Rt & 0b11111) << 16 // rs
               | (Fs & 0b11111) << 11; // rt
    }
}