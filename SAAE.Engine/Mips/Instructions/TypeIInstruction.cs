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

    protected PopulationOptions ParseOptions { get; init; }

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
            return short.Parse(text[2..], System.Globalization.NumberStyles.HexNumber);
        } else {
            return short.Parse(text, System.Globalization.NumberStyles.Integer);
        }
    }

    [Flags]
    protected enum PopulationOptions {
        None = 0,
        Rs = 1 << 0,
        Rt = 1 << 1,
        Immediate = 1 << 2,
        Offset = 1 << 3
    }

    public override void PopulateFromLine(string line) {
        var match = GetRegularExpression().Match(line);

        if(ParseOptions.HasFlag(PopulationOptions.Rs)) {
            if (byte.TryParse(match.Groups["rs"].Value, out byte rs)) {
                Rs = rs;
            } else {
                int regNum = TranslateRegisterName(match.Groups["rs"].Value);
                if (regNum < 0) {
                    throw new ArgumentException($"Invalid register name: {match.Groups["rs"].Value}");
                }
                Rs = (byte)regNum;
            }
        }

        if(ParseOptions.HasFlag(PopulationOptions.Rt)) {
            if (byte.TryParse(match.Groups["rt"].Value, out byte rt)) {
                Rt = rt;
            } else {
                int regNum = TranslateRegisterName(match.Groups["rt"].Value);
                if (regNum < 0) {
                    throw new ArgumentException($"Invalid register name: {match.Groups["rt"].Value}");
                }
                Rt = (byte)regNum;
            }
        }

        if(ParseOptions.HasFlag(PopulationOptions.Immediate)) {
            Immediate = ParseImmediate(match.Groups["immediate"].Value);
        }else if(ParseOptions.HasFlag(PopulationOptions.Offset)) {
            Immediate = ParseImmediate(match.Groups["offset"].Value);
        }

    }
}
