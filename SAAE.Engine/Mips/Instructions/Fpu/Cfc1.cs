using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,17)] // opcode
[FormatExact<Instruction>(25,21,2)] // rs
[FormatExact<Instruction>(10,6,0)] // shift
[FormatExact<Instruction>(5,0,0)] // funct
public class Cfc1 : TypeFInstruction
{
    public byte Rt { get; private set; }
    public byte Fs { get; private set; }

    public override string ToString() => $"cfc1 ${Instruction.TranslateRegisterName(Rt)}, ${TranslateRegisterName(Fs)}" + FormatTrivia();

    public override void FromInt(int instruction)
    {
        Rt = (byte)((instruction >> 16) & 0b11111);
        Fs = (byte)((instruction >> 11) & 0b11111);
    }

    public override int ConvertToInt()
    {
        return OpCode << 26 // opcode
               | 0b00010 << 21 // cf
               | (Rt & 0b11111) << 16 // rt
               | (Fs & 0b11111) << 11; // fs
    }
}