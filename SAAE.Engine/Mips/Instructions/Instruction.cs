using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions; 
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
    /// Returns the regular expression that should match the text of this instruction
    /// </summary>
    /// <returns></returns>
    public abstract Regex GetRegularExpression();

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

}
