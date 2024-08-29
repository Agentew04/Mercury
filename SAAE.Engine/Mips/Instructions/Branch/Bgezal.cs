using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Bgezal : TypeIInstruction {

    public Bgezal() {
        OpCode = 0b000001;
        Rt = 0b10001;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Offset;
    }

    [GeneratedRegex(@"^\s*bgez\s+\$(?<rs>\S+),\s*(?<offset>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
}
