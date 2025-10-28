using System.Text.RegularExpressions;
using SAAE.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(20,16,0)] // rt
[FormatExact<Instruction>(10,6,0)] // shift
[FormatExact<Instruction>(5,0,9)] // funct
public partial class Jalr2 : TypeRInstruction {

    public Jalr2() {
        OpCode = 0b000000;
        Rt = 0;
        ShiftAmount = 0;
        Function = 0b001_001;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rd;
    }

    [GeneratedRegex(@"^\s*jalr\s+\$(?<rd>[^\s,]+),\s*\$(?<rs>[^\s,]+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
