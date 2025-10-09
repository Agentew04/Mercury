using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,28)] // opcode
[FormatExact<Instruction>(5,0,4)] // funct
[FormatExact<Instruction>(15,11,0)] // rd
public partial class Msub : TypeRInstruction {

    public Msub() {
        Rd = 0;
        OpCode = 0b011100;
        Function = 0b100;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*msub\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
