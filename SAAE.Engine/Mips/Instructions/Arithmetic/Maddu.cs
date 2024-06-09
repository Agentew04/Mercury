using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Maddu : TypeRInstruction {

    public Maddu() {
        Rd = 0;
        OpCode = 0b011100;
        Function = 1;
    }

    [GeneratedRegex(@"^\s*maddu\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rs = byte.Parse(m.Groups["rs"].Value);
        Rt = byte.Parse(m.Groups["rt"].Value);
    }
}
