using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Mflo : TypeRInstruction {

    public Mflo() {
        Rs = 0;
        Rt = 0;
        OpCode = 0;
        Function = 0b10010;
        ParseOptions = PopulationOptions.Rd;
    }

    [GeneratedRegex(@"^\s*mflo\s+\$(?<rd>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}" + FormatTrivia();
}
