using System.Text.RegularExpressions;
using SAAE.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(5,0,12)] // funct
public partial class Syscall : TypeRInstruction {

    public Syscall() {
        Rd = 0;
        Rs = 0;
        Rt = 0;
        Function = 0b001100;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.None;
    }

    public int Code {
        get => (Rs << 15) | (Rt << 10) | (Rd << 5) | ShiftAmount;
        set {
            Rs = (byte)((value >> 15) & 0b11111);
            Rt = (byte)((value >> 10) & 0b11111);
            Rd = (byte)((value >> 5) & 0b11111);
            ShiftAmount = (byte)(value & 0b11111);
        }
    }

    [GeneratedRegex(@"^\s*syscall\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic}" + FormatTrivia();
}
