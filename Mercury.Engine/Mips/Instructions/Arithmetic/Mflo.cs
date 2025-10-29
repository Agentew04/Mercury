using System.Text.RegularExpressions;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(25,21,0)] // rs
[FormatExact<Instruction>(20,16,0)] // rt
[FormatExact<Instruction>(5,0,18)] // funct
public partial class Mflo : TypeRInstruction {

    public Mflo() {
        Rs = 0;
        Rt = 0;
        OpCode = 0;
        Function = 0b10010;
        ParseOptions = PopulationOptions.Rd;
    }

    [GeneratedRegex(@"^\s*mflo\s+\$(?<rd>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}" + FormatTrivia();
}
