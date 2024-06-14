using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Lw : TypeIInstruction {

    public Lw() {
        OpCode = 0b100011;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*lw\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\(\$(?<rs>\S+)\)\s*$")]
    public override partial Regex GetRegularExpression();
}
