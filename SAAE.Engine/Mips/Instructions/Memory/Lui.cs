using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Lui : TypeIInstruction {

    public Lui() {
        OpCode = 0b001111;
        Rs = 0;
    }

    [GeneratedRegex(@"^\s*lui\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        var match = GetRegularExpression().Match(line);
        Rt = byte.Parse(match.Groups["rt"].Value);
        Immediate = ParseImmediate(match.Groups["immediate"].Value);
    }
}
