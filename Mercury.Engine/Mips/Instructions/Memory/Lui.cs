using System.Text.RegularExpressions;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,15)] // opcode
[FormatExact<Instruction>(25,21,0)] // rs
public partial class Lui : TypeIInstruction {

    public Lui() {
        OpCode = 0b001111;
        Rs = 0;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*lui\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rt)}, {Immediate}" + FormatTrivia();
}
