namespace Mercury.Engine.Common;

/// <summary>
/// Interface that define common methods that all
/// instructions classes must implement, regardless of
/// instruction set architecture.
/// </summary>
public interface IInstruction {

    /// <summary>
    /// Reads values of significant fields from the
    /// binary representation of the instruction. 
    /// </summary>
    /// <param name="instruction">The binary representation of the instruction</param>
    public void FromInt(uint instruction);

    /// <summary>
    /// Converts this instruction into its binary representation.
    /// </summary>
    /// <returns>The binary form of the instruction</returns>
    public uint ConvertToInt();

    /// <summary>
    /// Returns a string representation of this instruction. 
    /// </summary>
    public string ToString();
}