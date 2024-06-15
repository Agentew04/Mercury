using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

/// <summary>
/// Trap if equal. Triggers a breakpoint if the contents of Rs and Rt are equal.
/// </summary>
public partial class Teq : TypeRInstruction {

    public Teq() {
        Function = 0b110100;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*teq\s+\$(?<rs>\S+),\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
