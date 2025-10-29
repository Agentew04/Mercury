using System.Text.RegularExpressions;
using SAAE.Generators;

namespace Mercury.Engine.Mips.Instructions;

/// <summary>
/// Trap if equal. Triggers a breakpoint if the contents of Rs and Rt are equal.
/// </summary>
[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(10,6,0)] // shift
[FormatExact<Instruction>(5,0,52)] // funct
public partial class Teq : TypeRInstruction {

    public Teq() {
        Function = 0b110100;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    public int Code {
        get => (Rd << 5) | ShiftAmount;
        set {
            Rd = (byte)((value >> 5) & 0b11111);
            ShiftAmount = (byte)(value & 0b11111);
        }
    }

    [GeneratedRegex(@"^\s*teq\s+\$(?<rs>\S+),\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
