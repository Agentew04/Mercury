using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Bne : TypeIInstruction {

    public Bne() {
        OpCode = 0b000101;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt | PopulationOptions.Offset;
    }

    [GeneratedRegex(@"^\s*bne\s+\$(?<rs>\S+),\s*\$(?<rt>\S+),\s*(?<offset>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}, {Immediate}" + FormatTrivia();
}
