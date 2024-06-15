using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Break : TypeRInstruction {

    public Break() {
        Rd = 0;
        Function = 0b001101;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.None;
    }

    [GeneratedRegex(@"^\s*break\s*$")]
    public override partial Regex GetRegularExpression();
}
