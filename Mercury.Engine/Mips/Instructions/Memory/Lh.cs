using System.Text.RegularExpressions;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,33)] // opcode
public partial class Lh : TypeIInstruction {

    public Lh() {
        OpCode = 0b100001;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*lh\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\(\$(?<rs>\S+)\)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rt)}, {Immediate}(${TranslateRegisterName(Rs)})" + FormatTrivia();
}
