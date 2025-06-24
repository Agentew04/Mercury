using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Mult : TypeRInstruction {

    public Mult() {
        Rd = 0;
        Function = 0b011000;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*mult\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
