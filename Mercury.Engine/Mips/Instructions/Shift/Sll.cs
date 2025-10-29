using System.Text.RegularExpressions;
using SAAE.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(25,21,0)] // rs
[FormatExact<Instruction>(5,0,0)] // funct
public partial class Sll : TypeRInstruction {

    public Sll() {
        Function = 0b000000;
        Rs = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rt | PopulationOptions.ShiftAmount;
    }

    [GeneratedRegex(@"^\s*sll\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*(?<shamt>\d+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rt)}, {ShiftAmount}" + FormatTrivia();
}
