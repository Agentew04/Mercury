using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Subu : TypeRInstruction {

    public Subu() {
        Function = 0x23;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"subu\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
