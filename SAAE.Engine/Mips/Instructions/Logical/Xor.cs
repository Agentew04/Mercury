using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(10,6,0)] // shift
[FormatExact<Instruction>(5,0,38)] // funct
public partial class Xor : TypeRInstruction {

    public Xor() {
        Function = 0b100110;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*xor\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
