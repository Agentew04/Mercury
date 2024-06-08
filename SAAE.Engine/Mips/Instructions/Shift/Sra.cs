using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions.Logical;

internal partial class Sra : TypeRInstruction {

    public Sra() {
        Function = 0b000011;
        Rs = 0;
    }

    [GeneratedRegex(@"sra\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*\$(?<shamt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
