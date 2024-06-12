using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Ori : TypeIInstruction {

    public Ori() {
        OpCode = 0b001101;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*ori\s+\$(?<rt>\S+),\s*\$(?<rs>\S+),\s*(?<immediate>([-+]?\d+)|((0x)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
}
