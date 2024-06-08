using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Sra : TypeRInstruction {

    public Sra() {
        Function = 0b000011;
        Rs = 0;
    }

    [GeneratedRegex(@"^\s*sra\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*(?<shamt>\d+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rd = byte.Parse(m.Groups["rd"].Value);
        Rt = byte.Parse(m.Groups["rt"].Value);
        ShiftAmount = byte.Parse(m.Groups["shamt"].Value);
    }
}
