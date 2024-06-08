using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Srav : TypeRInstruction {

    public Srav() {
        Function = 0b000111;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"^\s*srav\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rd = byte.Parse(m.Groups["rd"].Value);
        Rs = byte.Parse(m.Groups["rs"].Value);
        Rt = byte.Parse(m.Groups["rt"].Value);
    }
}
