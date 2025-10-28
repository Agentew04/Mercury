using System.Text.RegularExpressions;

namespace Mercury.Engine.Mips.Instructions;
public partial class Maddu : TypeRInstruction {

    public Maddu() {
        Rd = 0;
        OpCode = 0b011100;
        Function = 1;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*maddu\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
