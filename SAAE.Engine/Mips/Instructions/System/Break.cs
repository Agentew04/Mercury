using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Break : TypeRInstruction {

    public Break() {
        Rd = 0;
        Function = 0b001101;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"^\s*break\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        // empty
    }
}
