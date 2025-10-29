using System.Text.RegularExpressions;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(15,11,0)] // rd
[FormatExact<Instruction>(5,0,24)] // funct
public partial class Mult : TypeRInstruction {

    public Mult() {
        Rd = 0;
        Function = 0b011000;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*mult\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
