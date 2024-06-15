using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Mtlo : TypeRInstruction {

    public Mtlo() {
        Rt = 0;
        Rd = 0;
        OpCode = 0;
        Function = 0b010011;
        ParseOptions = PopulationOptions.Rs;
    }

    [GeneratedRegex(@"^\s*mtlo\s+\$(?<rs>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
}
