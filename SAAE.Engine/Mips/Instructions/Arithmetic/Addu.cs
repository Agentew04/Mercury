using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions; 

internal partial class Addu : TypeRInstruction {
    public Addu() {
        Function = 0x21;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"addu\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
