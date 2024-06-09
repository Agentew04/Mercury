using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SAAE.Engine.Mips.Instructions;

public abstract class TypeIInstruction : Instruction {

    public byte Rs { get; set; }

    /// <summary>
    /// The register that normally receives the result value
    /// </summary>
    public byte Rt { get; set; }

    /// <summary>
    /// A signed 16 bit immediate value
    /// </summary>
    public short Immediate { get; set; }

    public override int ConvertToInt() {
        return ((OpCode & 0x3F) << 26) | ((Rs & 0x1F) << 21) | ((Rt & 0x1F) << 16) | (Immediate & 0xFFFF);
    }

    public override void FromInt(int instruction) {
        OpCode = (byte)((instruction >> 26) & 0x3F);
        Rs = (byte)((instruction >> 21) & 0x1F);
        Rt = (byte)((instruction >> 16) & 0x1F);
        Immediate = (short)(instruction & 0xFFFF);
    }

    protected static short ParseImmediate(string text) {
        if (text.Contains('x') || text.Contains('X') 
            || text .StartsWith("0x") || text.StartsWith("0X")
            || text.Any(x => x >= 'A' && x <= 'F' || x >= 'a' && x <= 'f')) {
            return short.Parse(text, System.Globalization.NumberStyles.HexNumber);
        } else {
            return short.Parse(text, System.Globalization.NumberStyles.Integer);
        }
    }
}
