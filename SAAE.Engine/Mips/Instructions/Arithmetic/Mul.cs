using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

internal partial class Mul : TypeRInstruction {

    public Mul() {
        Function = 0x2;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"subu\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
