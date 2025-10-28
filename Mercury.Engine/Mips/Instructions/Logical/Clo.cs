using System.Text.RegularExpressions;
using SAAE.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,28)] // opcode
[FormatExact<Instruction>(20,16,0)] // rt
[FormatExact<Instruction>(10,6,0)] // shift
[FormatExact<Instruction>(5,0,33)] // funct
public partial class Clo : TypeRInstruction {

    public Clo() {
        OpCode = 0b11100;
        ShiftAmount = 0;
        Function = 0b100001;
        Rt = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs;
    }

    [GeneratedRegex(@"^\s*clo\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
