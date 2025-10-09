using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions; 

[FormatExact<Instruction>(31,26,0)]
[FormatExact<Instruction>(5,0,32)]
public partial class Add : TypeRInstruction {

    public Add() {
        Function = 0x20;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*add\s+\$(?<rd>\S+?)\s*,\s*\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();

    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
