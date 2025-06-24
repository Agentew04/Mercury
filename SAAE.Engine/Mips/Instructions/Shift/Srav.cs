using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Srav : TypeRInstruction {

    public Srav() {
        Function = 0b000111;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*srav\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rt)}, ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
