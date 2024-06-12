using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Sh : TypeIInstruction {

    public Sh() {
        OpCode = 0b101001;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*sh\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x)?[0-9A-Fa-f]+))\(\$(?<rs>\S+)\)\s*$")]
    public override partial Regex GetRegularExpression();
}
