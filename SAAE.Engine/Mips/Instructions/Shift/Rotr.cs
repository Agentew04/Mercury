using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Rotr : TypeRInstruction {

    public Rotr() {
        Function = 0b000010;
    }

    [GeneratedRegex(@"rotr\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*(?<shamt>\d+)\s*$")]
    public override partial Regex GetRegularExpression();
}
