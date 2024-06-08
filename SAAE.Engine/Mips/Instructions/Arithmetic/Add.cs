using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Instructions; 

public partial class Add : TypeRInstruction {

    public Add() {
        Function = 0x20;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"^\s*add\s+\$(?<rd>\S+?)\s*,\s*\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rd = byte.Parse(m.Groups["rd"].Value);
        Rs = byte.Parse(m.Groups["rs"].Value);
        Rt = byte.Parse(m.Groups["rt"].Value);
    }
}
