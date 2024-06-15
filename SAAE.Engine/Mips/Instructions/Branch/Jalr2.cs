using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Jalr2 : TypeRInstruction {

    public Jalr2() {
        OpCode = 0b000000;
        Rt = 0;
        Rd = 0b11111;
        ShiftAmount = 0;
        Function = 0b001_001;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*jalr\s+\$(?<rt>[^\s,]+),\s*\$(?<rs>[^\s,]+)\s*$")]
    public override partial Regex GetRegularExpression();
}
