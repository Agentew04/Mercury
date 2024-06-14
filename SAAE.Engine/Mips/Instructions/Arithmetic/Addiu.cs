using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Addiu : TypeIInstruction {

    public Addiu() {
        OpCode = 0b001001;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*addiu\s+\$(?<rt>\S+),\s*\$(?<rs>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
}
