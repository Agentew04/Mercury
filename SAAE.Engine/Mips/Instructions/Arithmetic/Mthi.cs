using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(20,6,0)] // rt
[FormatExact<Instruction>(5,0,17)] // funct
public partial class Mthi : TypeRInstruction {

    public Mthi() {
        Rt = 0;
        Rd = 0;
        OpCode = 0;
        Function = 0b010001;
        ParseOptions = PopulationOptions.Rs;
    }

    [GeneratedRegex(@"^\s*mthi\s+\$(?<rs>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
