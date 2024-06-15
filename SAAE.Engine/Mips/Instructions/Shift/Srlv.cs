using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Srlv : TypeRInstruction {

    public Srlv() {
        Function = 0b000110;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"\s*srlv\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
