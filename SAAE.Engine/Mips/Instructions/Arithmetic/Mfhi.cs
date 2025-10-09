using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(25,16,0)] // rs
[FormatExact<Instruction>(10,6,0)] // rt
[FormatExact<Instruction>(5,0,16)] // funct
public partial class Mfhi : TypeRInstruction {

    public Mfhi() {
        Rs = 0;
        Rt = 0;
        OpCode = 0;
        Function = 0b10000;
        ParseOptions = PopulationOptions.Rd;
    }

    [GeneratedRegex(@"^\s*mfhi\s+\$(?<rd>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}" + FormatTrivia();
}
