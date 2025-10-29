using System.Text.RegularExpressions;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,1)] // opcode
[FormatExact<Instruction>(20,16,1)] // rt
public partial class Bgez : TypeIInstruction {

    public Bgez() {
        OpCode = 0b000001;
        Rt = 0b00001;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Offset;
    }

    [GeneratedRegex(@"^\s*bgez\s+\$(?<rs>\S+),\s*(?<offset>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, 0x{Immediate:X4}" + FormatTrivia();
}
