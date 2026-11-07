using Mercury.Engine.Memory;

namespace Mercury.Engine.Common;

/// <summary>
/// Interface that defines methods for a class that is able to read bytes from an undefined source. 
/// </summary>
public interface IInstructionByteSource {
    
    /// <summary>
    /// The endianness of the byte sequence provided by this source.
    /// </summary>
    Endianness Endianness { get; }

    /// <summary>
    /// Tries to read one byte from the source.
    /// </summary>
    /// <param name="offset">The address offset, relative to the inner stream's start</param>
    /// <param name="value">The byte that was read</param>
    /// <returns>True if a byte was read successfully or false if not</returns>
    bool TryRead(long offset, out byte value);
}