using System.Text.RegularExpressions;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,28)] // opcode
[FormatExact<Instruction>(5,0,5)] // funct
[FormatExact<Instruction>(15,11,0)] // rd
public partial class Msubu : TypeRInstruction {

    public Msubu() {
        Rd = 0;
        OpCode = 0b011100;
        Function = 0b101;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*msubu\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
