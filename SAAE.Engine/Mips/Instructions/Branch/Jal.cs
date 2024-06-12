using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

/// <summary>
/// Jump and link instruction. Jumps to the target address and stores the return address in $ra register.
/// </summary>
public partial class Jal : TypeJInstruction {

    public Jal() {
        OpCode = 0b000011;
    }

    [GeneratedRegex(@"^\s*jal\s+(?<target>(0x)?[0-9A-Fa-f]+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        var match = GetRegularExpression().Match(line);
        Target = ParseImmediate(match.Groups["target"].Value);
    }
}
