using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Instructions;

/// <summary>
/// Jump to register instruction. Jumps to the address stored in the register provided.
/// </summary>
public partial class Jr : TypeRInstruction {

    public Jr() {
        OpCode = 0b000000;
        Rt = 0;
        Rd = 0;
        ShiftAmount = 0;
        Function = 0b001_000;
    }

    [GeneratedRegex(@"^\s*jr\s+\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rs = byte.Parse(m.Groups["rs"].Value);
    }
}
