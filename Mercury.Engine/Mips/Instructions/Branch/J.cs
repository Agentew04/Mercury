using System.Text.RegularExpressions;

namespace Mercury.Engine.Mips.Instructions;

/// <summary>
/// The unconditional jump instruction
/// </summary>
public partial class J : TypeJInstruction {

    public J() {
        OpCode = 0b000010;
    }

    [GeneratedRegex(@"^\s*j\s+(?<target>(0x|0X)?[0-9A-Fa-f]+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString(byte highOrderPc) => $"{Mnemonic} 0x{(highOrderPc << 26) | (Immediate << 2):X7}" + FormatTrivia();
}
