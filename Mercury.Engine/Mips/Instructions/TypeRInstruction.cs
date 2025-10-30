using System.Text.RegularExpressions;

namespace Mercury.Engine.Mips.Instructions;

public abstract class TypeRInstruction : Instruction {

    /// <summary>
    /// The register that normally receives the result of the operation.
    /// </summary>
    public byte Rd { get; set; }

    public byte Rs { get; set; }

    public byte Rt { get; set; }

    /// <summary>
    /// How many bits the value is shifted. Used on shift instructions.
    /// </summary>
    public byte ShiftAmount { get; set; }

    /// <summary>
    /// Which function of the R-Type instruction is being executed.
    /// </summary>
    public byte Function { get; protected set; }


    protected PopulationOptions ParseOptions { get; init; }

    public override int ConvertToInt() {
        return ((OpCode & 0x3F) << 26) | ((Rs & 0x1F) << 21) | ((Rt & 0x1F) << 16) | ((Rd & 0x1F) << 11) | ((ShiftAmount & 0x1F) << 6) | (Function & 0x3F);
    }

    public override void FromInt(int instruction) {
        OpCode = (byte)((instruction >> 26) & 0x3F);
        Rs = (byte)((instruction >> 21) & 0x1F);
        Rt = (byte)((instruction >> 16) & 0x1F);
        Rd = (byte)((instruction >> 11) & 0x1F);
        ShiftAmount = (byte)((instruction >> 6) & 0x1F);
        Function = (byte)(instruction & 0x3F);
    }

    [Flags]
    protected enum PopulationOptions {
        None = 0,
        Rs = 1 << 0,
        Rt = 1 << 1,
        Rd = 1 << 2,
        ShiftAmount = 1 << 3
    }

    public override void PopulateFromLine(string line) {
        Match? match = GetRegularExpression().Match(line);

        if (ParseOptions.HasFlag(PopulationOptions.Rs)) {
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

        if (ParseOptions.HasFlag(PopulationOptions.Rt)) {
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

        if (ParseOptions.HasFlag(PopulationOptions.Rd)) {
            if (byte.TryParse(match.Groups["rd"].Value, out byte rt)) {
                Rt = rt;
            } else {
                int regNum = TranslateRegisterName(match.Groups["rd"].Value);
                if (regNum < 0) {
                    throw new ArgumentException($"Invalid register name: {match.Groups["rt"].Value}");
                }
                Rt = (byte)regNum;
            }
        }

        if (ParseOptions.HasFlag(PopulationOptions.ShiftAmount)) {
            ShiftAmount = (byte)ParseImmediate(match.Groups["shamt"].Value);
        }
    }

    
}
