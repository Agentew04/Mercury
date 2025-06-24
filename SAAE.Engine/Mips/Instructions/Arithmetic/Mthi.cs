using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Mthi : TypeRInstruction {

    public Mthi() {
        Rt = 0;
        Rd = 0;
        OpCode = 0;
        Function = 0b010001;
        ParseOptions = PopulationOptions.Rs;
    }

    [GeneratedRegex(@"^\s*mthi\s+\$(?<rs>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
