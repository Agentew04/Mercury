using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Madd : TypeRInstruction {

    public Madd() {
        Rd = 0;
        OpCode = 0b011100;
        Function = 0;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*madd\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
}
