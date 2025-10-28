using System.Text.RegularExpressions;

namespace Mercury.Engine.Mips.Instructions;
public partial class Slti : TypeIInstruction {

    public Slti() {
        OpCode = 0b001010;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*slti\s+\$(?<rt>\S+),\s*\$(?<rs>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rt)}, ${TranslateRegisterName(Rs)}, {Immediate}" + FormatTrivia();
}
