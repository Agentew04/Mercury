using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mercury.Engine.Mips.Instructions; 
internal partial class Nop : Instruction {
    public override int ConvertToInt() {
        return 0;
    }

    public override void FromInt(int instruction) {
        // do nothing
    }

    [GeneratedRegex(@"nop\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        // do nothing
    }
    
    public override string ToString() => $"{Mnemonic}" + FormatTrivia();
}
