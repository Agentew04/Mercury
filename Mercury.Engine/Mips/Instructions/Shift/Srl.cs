using System.Text.RegularExpressions;

namespace Mercury.Engine.Mips.Instructions;

public partial class Srl : TypeRInstruction {

    public Srl() {
        Function = 0b000010;
        Rs = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rt | PopulationOptions.ShiftAmount;
    }

    [GeneratedRegex(@"^\s*srl\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*(?<shamt>\d+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rt)}, {ShiftAmount}" + FormatTrivia();
}
