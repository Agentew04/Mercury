using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Sltu : TypeRInstruction {

    public Sltu() {
        Function = 0b101011;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"^\s*sltu\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rd = byte.Parse(m.Groups["rd"].Value);
        Rs = byte.Parse(m.Groups["rs"].Value);
        Rt = byte.Parse(m.Groups["rt"].Value);
    }
}
