using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Slti : TypeIInstruction {

    public Slti() {
        OpCode = 0b001010;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*slti\s+\$(?<rt>\S+),\s*\$(?<rs>\S+),\s*(?<immediate>([-+]?\d+)|((0x)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
}
