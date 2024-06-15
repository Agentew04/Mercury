using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Slt : TypeRInstruction {

    public Slt() {
        Function = 0b101010;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*slt\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
