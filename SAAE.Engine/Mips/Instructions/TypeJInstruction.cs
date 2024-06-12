namespace SAAE.Engine.Mips.Instructions; 

public abstract class TypeJInstruction : Instruction{

    /// <summary>
    /// A signed 26 bit target address value
    /// </summary>
    public int Target {
        get => target & 0x3FFFFFF;
        set => target = value & 0x3FFFFFF;
    }
    private int target = 0;

    public override int ConvertToInt() {
        return ((OpCode & 0x3F) << 26) | (Target & 0x3FFFFFF);
    }

    public override void FromInt(int instruction) {
        OpCode = (byte)((instruction >> 26) & 0x3F);
        Target = instruction & 0x3FFFFFF;
    }

    protected static int ParseImmediate(string text) {
        if (text.Contains('x') || text.Contains('X')
            || text.StartsWith("0x") || text.StartsWith("0X")
            || text.Any(x => x >= 'A' && x <= 'F' || x >= 'a' && x <= 'f')) {
            return int.Parse(text[2..], System.Globalization.NumberStyles.HexNumber);
        } else {
            return int.Parse(text, System.Globalization.NumberStyles.Integer);
        }
    }
}
