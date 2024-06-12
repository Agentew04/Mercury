using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Lbu : TypeIInstruction {

    public Lbu() {
        OpCode = 0b100100;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*lbu\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x)?[0-9A-Fa-f]+))\(\$(?<rs>\S+)\)\s*$")]
    public override partial Regex GetRegularExpression();
}
