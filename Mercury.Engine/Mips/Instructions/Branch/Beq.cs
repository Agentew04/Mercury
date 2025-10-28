using System.Text.RegularExpressions;

namespace Mercury.Engine.Mips.Instructions;
public partial class Beq : TypeIInstruction {

    public Beq() {
        OpCode = 0b000100;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt | PopulationOptions.Offset;
    }

    [GeneratedRegex(@"^\s*beq\s+\$(?<rs>\S+),\s*\$(?<rt>\S+),\s*(?<offset>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}, 0x{Immediate:X4}" + FormatTrivia();
}
