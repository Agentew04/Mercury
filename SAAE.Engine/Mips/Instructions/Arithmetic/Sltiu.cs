using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Sltiu : TypeIInstruction {

    public Sltiu() {
        OpCode = 0b001011;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*sltiu\s+\$(?<rt>\S+),\s*\$(?<rs>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rt)}, ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
