using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Bgtz : TypeIInstruction {

    public Bgtz() {
        OpCode = 0b000111;
        Rt = 0b00000;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Offset;
    }

    [GeneratedRegex(@"^\s*bgtz\s+\$(?<rs>\S+),\s*(?<offset>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
}
