using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Lb : TypeIInstruction {

    public Lb() {
        OpCode = 0b100000;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*lb\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\(\$(?<rs>\S+)\)\s*$")]
    public override partial Regex GetRegularExpression();
}
