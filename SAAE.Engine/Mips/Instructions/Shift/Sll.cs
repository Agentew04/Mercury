using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions.Logical;

internal partial class Sll : TypeRInstruction {

    public Sll() {
        Function = 0b000000;
        Rd = 0;
    }

    [GeneratedRegex(@"sll\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*\$(?<shamt>\d+)\s*$")]
    public override partial Regex GetRegularExpression();
}
