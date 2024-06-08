using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

internal partial class Sub : TypeRInstruction {

    public Sub() {
        Function = 0x22;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"sub\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
