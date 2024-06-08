using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions.Logical;

internal partial class Xor : TypeRInstruction {

    public Xor() {
        Function = 0b100110;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"xor\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
