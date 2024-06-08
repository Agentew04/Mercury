using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

internal partial class Sltu : TypeRInstruction {

    public Sltu() {
        Function = 0b101011;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"sltu\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
