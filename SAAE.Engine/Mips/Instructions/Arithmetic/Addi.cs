using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Addi : TypeIInstruction {

    public Addi() {
        OpCode = 0b001000;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*addi\s+\$(?<rt>\S+),\s*\$(?<rs>\S+),\s*(?<immediate>([-+]?\d+)|((0x)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
}
