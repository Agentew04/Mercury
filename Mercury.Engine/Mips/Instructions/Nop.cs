using System.Text.RegularExpressions;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,0,0)]
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
