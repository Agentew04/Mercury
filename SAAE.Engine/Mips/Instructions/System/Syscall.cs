using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Syscall : TypeRInstruction {

    public Syscall() {
        Rd = 0;
        Rs = 0;
        Rt = 0;
        Function = 0b001100;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.None;
    }

    [GeneratedRegex(@"^\s*syscall\s*$")]
    public override partial Regex GetRegularExpression();
}
