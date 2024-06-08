using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Mul : TypeRInstruction {

    public Mul() {
        OverrideOpCode(0b011100);
        Function = 0x2;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"mul\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
