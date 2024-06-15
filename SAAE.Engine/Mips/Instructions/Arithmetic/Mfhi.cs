using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Mfhi : TypeRInstruction {

    public Mfhi() {
        Rs = 0;
        Rt = 0;
        OpCode = 0;
        Function = 0b10000;
        ParseOptions = PopulationOptions.Rd;
    }

    [GeneratedRegex(@"^\s*mfhi\s+\$(?<rd>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
}
