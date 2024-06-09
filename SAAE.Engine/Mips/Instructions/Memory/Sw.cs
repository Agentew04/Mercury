using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Sw : TypeIInstruction {

    public Sw() {
        OpCode = 0b101011;
    }

    [GeneratedRegex(@"^\s*sw\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x)?[0-9A-Fa-f]+))\(\$(?<rs>\S+)\)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        var match = GetRegularExpression().Match(line);
        Rt = byte.Parse(match.Groups["rt"].Value);
        Rs = byte.Parse(match.Groups["rs"].Value);
        Immediate = ParseImmediate(match.Groups["immediate"].Value);
    }
}
