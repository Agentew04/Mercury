using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Sllv : TypeRInstruction {

    public Sllv() {
        Function = 0b000100;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*sllv\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
