using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Movn : TypeRInstruction {

    public Movn() {
        OpCode = 0;
        Function = 0b001_011;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*movn\s+\$(?<rd>\S+?)\s*,\s*\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
}
