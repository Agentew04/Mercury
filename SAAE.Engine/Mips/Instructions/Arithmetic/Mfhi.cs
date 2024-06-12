using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Mfhi : TypeRInstruction {

    public Mfhi() {
        Rs = 0;
        Rt = 0;
        OpCode = 0;
        Function = 0b10000;
    }

    [GeneratedRegex(@"^\s*mfhi\s+\$(?<rd>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rd = byte.Parse(m.Groups["rd"].Value);
    }
}
