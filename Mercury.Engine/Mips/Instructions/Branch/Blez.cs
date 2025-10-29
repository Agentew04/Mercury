using System.Text.RegularExpressions;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Instructions;

[FormatExact<Instruction>(31,26,6)] // opcode
[FormatExact<Instruction>(20,16,0)] // rt
public partial class Blez : TypeIInstruction {

    public Blez() {
        OpCode = 0b000_110;
        Rt = 0b00_000;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Offset;
    }

    [GeneratedRegex(@"^\s*blez\s+\$(?<rs>\S+),\s*(?<offset>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, 0x{Immediate:X4}" + FormatTrivia();
}
