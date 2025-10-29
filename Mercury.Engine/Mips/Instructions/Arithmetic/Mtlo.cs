using System.Text.RegularExpressions;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(20,6,0)] // rt
[FormatExact<Instruction>(5,0,19)] // funct
public partial class Mtlo : TypeRInstruction {

    public Mtlo() {
        Rt = 0;
        Rd = 0;
        OpCode = 0;
        Function = 0b010011;
        ParseOptions = PopulationOptions.Rs;
    }

    [GeneratedRegex(@"^\s*mtlo\s+\$(?<rs>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
