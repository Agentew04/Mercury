using System.Text.RegularExpressions;

namespace Mercury.Engine.Mips.Instructions; 
public abstract class Instruction {

    /// <summary>
    /// The opcode of this instruction
    /// </summary>
    public byte OpCode { get; protected set; } = 0;

    /// <summary>
    /// Where this instruction is located in memory
    /// </summary>
    public int? Address { get; set; }

    /// <summary>
    /// An optional comment that may be on a instruction line
    /// </summary>
    public string CommentTrivia { get; set; } = "";

    /// <summary>
    /// Returns the regular expression that should match the text of this instruction
    /// </summary>
    /// <returns></returns>
    public abstract Regex GetRegularExpression();

    public abstract override string ToString();

    /// <summary>
    /// Checks if the opcode and function matches this instruction
    /// </summary>
    /// <param name="line">The line to check against</param>
    /// <returns></returns>
    public bool IsMatch(string line) {
        return GetRegularExpression().IsMatch(line);
    }

    /// <summary>
    /// Gets instruction details and data from a string
    /// </summary>
    /// <param name="line"></param>
    public abstract void PopulateFromLine(string line);

    /// <summary>
    /// Disassembles this instruction from an 4 byte integer
    /// </summary>
    /// <param name="instruction">The integer that contains this instruction data</param>
    public abstract void FromInt(int instruction);

    /// <summary>
    /// Assembles this instruction into a 4 byte integer
    /// </summary>
    /// <returns>The assembled instruction</returns>
    public abstract int ConvertToInt();

    private static readonly string[] Names = [
        "zero", "at", "v0", "v1", "a0", "a1", "a2", "a3", "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7", 
        "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7", "t8", "t9", "k0", "k1", "gp", "sp", "fp", "ra"
    ];
    
    public static int TranslateRegisterName(string name) {
        return Array.IndexOf(Names, name);
    }
    
    public static string TranslateRegisterName(int index)
    {
        return Names[index];
    }

    protected string FormatTrivia()
    {
        return string.IsNullOrWhiteSpace(CommentTrivia) ? string.Empty : " // " + CommentTrivia;
    }

    protected string Mnemonic => GetType().Name.ToLowerInvariant();

    /// <summary>
    /// Parses a string of a number.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    protected static short ParseImmediate(string text) {
        if (text.Contains('x') || text.Contains('X')
            || text.StartsWith("0x") || text.StartsWith("0X")
            || text.Any(x => x >= 'A' && x <= 'F' || x >= 'a' && x <= 'f')) {
            return short.Parse(text[2..], System.Globalization.NumberStyles.HexNumber);
        } else {
            return short.Parse(text, System.Globalization.NumberStyles.Integer);
        }
    }
}
