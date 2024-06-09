using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Ori : TypeIInstruction {

    public Ori() {
        OpCode = 0b001101;
    }

    [GeneratedRegex(@"^\s*ori\s+\$(?<rt>\S+),\s*\$(?<rs>\S+),\s*(?<immediate>([-+]?\d+)|((0x)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        var match = GetRegularExpression().Match(line);
        Rt = byte.Parse(match.Groups["rt"].Value);
        Rs = byte.Parse(match.Groups["rs"].Value);
        Immediate = ParseImmediate(match.Groups["immediate"].Value);
    }
}
