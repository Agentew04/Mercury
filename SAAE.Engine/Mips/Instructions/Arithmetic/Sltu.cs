using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(5,0,43)] // funct
[FormatExact<Instruction>(10,6,0)] // shamt
public partial class Sltu : TypeRInstruction {

    public Sltu() {
        Function = 0b101011;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*sltu\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
