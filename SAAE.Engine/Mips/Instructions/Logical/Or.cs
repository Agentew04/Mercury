using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions.Logical;

internal partial class Or : TypeRInstruction {

    public Or() {
        Function = 0b100101;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"or\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
