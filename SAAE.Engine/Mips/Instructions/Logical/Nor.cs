using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions.Logical;

internal partial class Nor : TypeRInstruction {

    public Nor() {
        Function = 0b100111;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"nor\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
