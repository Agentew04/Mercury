using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Clz : TypeRInstruction {

    public Clz() {
        OpCode = 0b011_100;
        ShiftAmount = 0;
        Function = 0b100_000;
        Rt = 0;
    }

    [GeneratedRegex(@"^\s*clz\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rd = byte.Parse(m.Groups["rd"].Value);
        Rs = byte.Parse(m.Groups["rs"].Value);
    }
}
