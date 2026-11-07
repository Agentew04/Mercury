using Mercury.Engine.Memory;

namespace Mercury.Engine.Common;

/// <summary>
/// Interface that define common methods that all
/// instructions classes must implement, regardless of
/// instruction set architecture.
/// </summary>
public interface IInstruction {
    
    /// <summary>
    /// Returns the size of the encoded instruction in bytes. 
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Reads instruction fields from a byte sequence.
    /// </summary>
    /// <param name="bytes">A span with the bytes to read from</param>
    /// <param name="endianness">The endianness of the byte sequence</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the span is smaller than the size
    /// of the encoded instruction</exception>
    public void FromBytes(ReadOnlySpan<byte> bytes, Endianness endianness);
    
    /// <summary>
    /// Converts the instruction into its binary encoded representation.
    /// </summary>
    /// <param name="bytes">The span to write the bytes to</param>
    /// <param name="endianness">The endianness of the byte sequence</param>
    public void ToBytes(Span<byte> bytes, Endianness endianness);

    /// <summary>
    /// Tries to convert the instruction to a 32-bit integer binary representation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Size"/> is different from 4</exception>
    /// <returns>The instruction encoded as a 32-bit integer</returns>
    public uint ConvertToInt(Endianness endianness);

    /// <summary>
    /// Returns a string representation of this instruction. 
    /// </summary>
    public string? ToString();
}