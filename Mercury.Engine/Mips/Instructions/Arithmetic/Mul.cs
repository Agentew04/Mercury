using System.Text.RegularExpressions;

namespace Mercury.Engine.Mips.Instructions;

public partial class Mul : TypeRInstruction {

    public Mul() {
        OpCode = 0b011100;
        Function = 0x2;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*mul\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rs)}, ${TranslateRegisterName(Rt)}" + FormatTrivia();
}
