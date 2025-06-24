using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Multu : TypeRInstruction {

    public Multu() {
        Rd = 0;
        Function = 0b011001;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*multu\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
