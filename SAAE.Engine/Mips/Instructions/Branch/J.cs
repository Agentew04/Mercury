using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

/// <summary>
/// The unconditional jump instruction
/// </summary>
public partial class J : TypeJInstruction {

    public J() {
        OpCode = 0b000010;
    }

    [GeneratedRegex(@"^\s*j\s+(?<target>(0x)?[0-9A-Fa-f]+)\s*$")]
    public override partial Regex GetRegularExpression();
}
