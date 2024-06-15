using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class And : TypeRInstruction {

    public And() {
        Function = 0b100100;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*and\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
