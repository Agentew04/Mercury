using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(10,6,0)] // shift
[FormatExact<Instruction>(5,0,6)] // funct
public partial class Srlv : TypeRInstruction {

    public Srlv() {
        Function = 0b000110;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"\s*srlv\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rt)}, ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
