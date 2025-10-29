using System.Text.RegularExpressions;
using SAAE.Generators;

namespace Mercury.Engine.Mips.Instructions; 

[FormatExact<Instruction>(31,26,0)]
[FormatExact<Instruction>(10,6,0)]
[FormatExact<Instruction>(5,0,33)]
public partial class Addu : TypeRInstruction {
    public Addu() {
        Function = 0x21;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*addu\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
