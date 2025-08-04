using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Bltzal : TypeIInstruction {

    public Bltzal() {
        OpCode = 0b000001;
        Rt = 0b10_000;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Offset;
    }

    [GeneratedRegex(@"^\s*bltz\s+\$(?<rs>\S+),\s*(?<offset>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, 0x{Immediate:X4}" + FormatTrivia();
}
