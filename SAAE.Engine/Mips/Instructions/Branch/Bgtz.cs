using System.Text.RegularExpressions;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,7)] // opcode
[FormatExact<Instruction>(20,16,0)] // rt
public partial class Bgtz : TypeIInstruction {

    public Bgtz() {
        OpCode = 0b000111;
        Rt = 0b00000;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Offset;
    }

    [GeneratedRegex(@"^\s*bgtz\s+\$(?<rs>\S+),\s*(?<offset>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, 0x{Immediate:X4}" + FormatTrivia();
}
