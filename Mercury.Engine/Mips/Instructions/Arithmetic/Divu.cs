using System.Text.RegularExpressions;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)]
[FormatExact<Instruction>(15,11,0)]
[FormatExact<Instruction>(10,6,0)]
[FormatExact<Instruction>(5,0,27)]
public partial class Divu : TypeRInstruction {

    public Divu() {
        Rd = 0;
        Function = 0b011011;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*divu\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
