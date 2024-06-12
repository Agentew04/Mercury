using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Bgez : TypeIInstruction {

    public Bgez() {
        OpCode = 0b000001;
        Rt = 0b00001;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Offset;
    }

    [GeneratedRegex(@"^\s*bgez\s+\$(?<rs>\S+),\s*(?<offset>([-+]?\d+)|((0x)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
}
