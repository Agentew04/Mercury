using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Lbu : TypeIInstruction {

    public Lbu() {
        OpCode = 0b100100;
    }

    [GeneratedRegex(@"^\s*lbu\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x)?[0-9A-Fa-f]+))\(\$(?<rs>\S+)\)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        var match = GetRegularExpression().Match(line);
        Rt = byte.Parse(match.Groups["rt"].Value);
        Rs = byte.Parse(match.Groups["rs"].Value);
        Immediate = ParseImmediate(match.Groups["immediate"].Value);
    }
}
