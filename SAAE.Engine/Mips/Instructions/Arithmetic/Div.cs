using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)]
[FormatExact<Instruction>(15,11,0)]
[FormatExact<Instruction>(10,6,0)]
[FormatExact<Instruction>(5,0,26)]
public partial class Div : TypeRInstruction {

    public Div() {
        Rd = 0;
        Function = 0b011010;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*div\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
