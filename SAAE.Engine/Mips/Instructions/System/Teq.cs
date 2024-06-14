using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions.System;

/// <summary>
/// Trap if equal. Triggers a breakpoint if the contents of Rs and Rt are equal.
/// </summary>
public partial class Teq : TypeRInstruction {

    public Teq() {
        Function = 0b110100;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"^\s*teq\s+\$(?<rs>\S+),\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rs = byte.Parse(m.Groups["rs"].Value);
        Rt = byte.Parse(m.Groups["rt"].Value);
    }
}
