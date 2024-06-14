using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Blez : TypeIInstruction {

    public Blez() {
        OpCode = 0b000_110;
        Rt = 0b00_000;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Offset;
    }

    [GeneratedRegex(@"^\s*blez\s+\$(?<rs>\S+),\s*(?<offset>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
}
