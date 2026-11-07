using System.Buffers.Binary;
using Mercury.Engine.Memory;

namespace Mercury.Engine.Common;

/// <summary>
/// Implementation of <see cref="IInstructionByteSource"/> but only reads 4 bytes at a time.
/// </summary>
public class Int32ByteSource : IInstructionByteSource {

    private uint instruction;

    public Int32ByteSource(Endianness endianness) {
        Endianness = endianness;
    }
    
    public Int32ByteSource(uint instruction, Endianness endianness) {
        this.instruction = instruction;
        Endianness = endianness;
    }

    /// <summary>
    /// Defines which will be the new instruction to be used in this InstructionSource
    /// </summary>
    /// <param name="newInstruction"></param>
    public void SetInstruction(uint newInstruction) {
        instruction = newInstruction;
    }
    
    public Endianness Endianness { get; }
    public bool TryRead(long offset, out byte value) {
        if (offset is < 0 or >= 4) {
            value = 0;
            return false;
        }

        Span<byte> buffer = stackalloc byte[4];
        if (Endianness == Endianness.BigEndian) {
            BinaryPrimitives.WriteUInt32BigEndian(buffer, instruction);
        }
        else {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, instruction);
        }

        value = buffer[(int)offset];
        return true;
    }
}