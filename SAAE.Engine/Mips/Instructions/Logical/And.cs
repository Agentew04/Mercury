using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class And : TypeRInstruction {

    public And() {
        Function = 0b100100;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"and\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
