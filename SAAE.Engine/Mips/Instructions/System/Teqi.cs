using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Teqi : TypeIInstruction {

    public Teqi() {
        OpCode = 0b000001;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Immediate;
        Rt = 0b01100;
    }

    [GeneratedRegex(@"^\s*teqi\s+\$(?<rs>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, {Immediate}" + FormatTrivia();
}
