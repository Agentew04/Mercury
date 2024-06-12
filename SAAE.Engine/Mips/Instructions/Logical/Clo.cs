using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Clo : TypeRInstruction {

    public Clo() {
        OpCode = 0b11100;
        ShiftAmount = 0;
        Function = 0b100001;
        Rt = 0;
    }

    [GeneratedRegex(@"^\s*clo\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rd = byte.Parse(m.Groups["rd"].Value);
        Rs = byte.Parse(m.Groups["rs"].Value);
    }
}
