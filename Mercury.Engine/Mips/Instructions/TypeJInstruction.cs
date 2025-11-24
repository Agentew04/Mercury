using System.Text.RegularExpressions;

namespace Mercury.Engine.Mips.Instructions; 

public abstract class TypeJInstruction : Instruction{

    /// <summary>
    /// The 26 lower bits shifted 2 to the left
    /// of the target address.
    /// </summary>
    public int Immediate {
        get => target & 0x3FFFFFF;
        set => target = value & 0x3FFFFFF;
    }
    private int target;

    public override int ConvertToInt() {
        return ((OpCode & 0x3F) << 26) | ((Immediate) & 0x3FFFFFF);
    }

    public override void FromInt(int instruction) {
        OpCode = (byte)((instruction >> 26) & 0x3F);
        Immediate = (instruction & 0x3FFFFFF);
    }

    private new static int ParseImmediate(string text) {
        if (text.Contains('x') || text.Contains('X')
                               || text.StartsWith("0x") || text.StartsWith("0X")
                               || text.Any(x => x >= 'A' && x <= 'F' || x >= 'a' && x <= 'f')) {
            return int.Parse(text[2..], System.Globalization.NumberStyles.HexNumber);
        }

        return int.Parse(text, System.Globalization.NumberStyles.Integer);
    }

    public override void PopulateFromLine(string line) {
        Match match = GetRegularExpression().Match(line);
        Immediate = ParseImmediate(match.Groups["target"].Value);
    }

    public abstract string ToString(byte highOrderPc);

    public override string ToString() => ToString(0);
}
