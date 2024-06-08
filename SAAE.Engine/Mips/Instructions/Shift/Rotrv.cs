using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Rotrv : TypeRInstruction {

    public Rotrv() {
        Function = 0b000110;
        ShiftAmount = 0b00001;
    }

    [GeneratedRegex(@"rotrv\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
