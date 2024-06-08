using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Rotrv : TypeRInstruction {

    public Rotrv() {
        Function = 0b000110;
        ShiftAmount = 0b00001;
    }

    [GeneratedRegex(@"^\s*rotrv\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rd = byte.Parse(m.Groups["rd"].Value);
        Rs = byte.Parse(m.Groups["rs"].Value);
        Rt = byte.Parse(m.Groups["rt"].Value);
    }
}
