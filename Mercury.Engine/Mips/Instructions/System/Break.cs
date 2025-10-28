using System.Text.RegularExpressions;

namespace Mercury.Engine.Mips.Instructions;

public partial class Break : TypeRInstruction {

    public Break() {
        Rd = 0;
        Function = 0b001101; // BREAK
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.None;
    }

    public int Code {
        get => (Rs << 15) | (Rt << 10) | (Rd << 5) | ShiftAmount;
        set {
            Rs = (byte)((value >> 15) & 0b11111);
            Rt = (byte)((value >> 10) & 0b11111);
            Rd = (byte)((value >> 5) & 0b11111);
            ShiftAmount = (byte)(value & 0b11111);
        }
    }

    [GeneratedRegex(@"^\s*break\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic}" + FormatTrivia();
}
