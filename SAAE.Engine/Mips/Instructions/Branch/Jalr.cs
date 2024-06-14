using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Instructions;

/// <summary>
/// Jump and link register. Jumps to the address stored in the register provided and stores the return address in $ra register or
/// an specific one.
/// </summary>
public partial class Jalr : TypeRInstruction {

    public Jalr() {
        OpCode = 0b000000;
        Rt = 0;
        Rd = 0b11111;
        ShiftAmount = 0;
        Function = 0b001_001;
    }

    [GeneratedRegex(@"^\s*jalr\s+(?:\$(?<rt>[^\s,]+),)?\s*\$(?<rs>[^\s,]+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rs = byte.Parse(m.Groups["rs"].Value);
    }
}
