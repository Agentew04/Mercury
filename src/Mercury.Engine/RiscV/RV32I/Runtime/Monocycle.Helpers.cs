using System.Buffers.Binary;
using Mercury.Engine.Common;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory;

namespace Mercury.Engine.RiscV.RV32I.Runtime;

public partial class Monocycle {
    /// <summary>
    /// Fills the <see cref="instructionBuffer"/> with the first <paramref name="size"/> bytes of the
    /// instruction read from <paramref name="source"/>, zero padding the rest.
    /// </summary>
    private void FillInstructionBuffer(IInstructionByteSource source, int size) {
        for (int i = 0; i < instructionBuffer.Length; i++) {
            instructionBuffer.Span[i] = i < size && source.TryRead(i, out byte b) ? b : (byte)0;
        }
    }

    /// <summary>
    /// Best-effort reconstruction of the instruction word (up to 4 bytes) for event reporting.
    /// </summary>
    private uint ReadFetchedWord(IInstructionByteSource source, int size) {
        Span<byte> bytes = stackalloc byte[4];
        for (int i = 0; i < 4; i++) {
            bytes[i] = i < size && source.TryRead(i, out byte b) ? b : (byte)0;
        }
        return endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadUInt32LittleEndian(bytes)
            : BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    private void ReadMemory(ulong address, Memory<byte> buffer) {
        MemoryReadEvent read = new() {
            Address = address,
            Buffer = buffer,
            Size = (ulong)buffer.Length
        };
        eventBus.Publish(read);
    }
    
    private int BytesToInt32(ReadOnlySpan<byte> word) {
        return endianness == Endianness.LittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(word) : BinaryPrimitives.ReadInt32BigEndian(word);
    }

    private short BytesToInt16(ReadOnlySpan<byte> word) {
        return endianness == Endianness.LittleEndian ? BinaryPrimitives.ReadInt16LittleEndian(word) : BinaryPrimitives.ReadInt16BigEndian(word);
    }

    private void Int32ToBytes(int value, Span<byte> destination) {
        if (endianness == Endianness.LittleEndian) {
            BinaryPrimitives.WriteInt32LittleEndian(destination, value);
        }
        else {
            BinaryPrimitives.WriteInt32BigEndian(destination, value);
        }
    }

    private void Int16ToBytes(short value, Span<byte> destination) {
        if (endianness == Endianness.LittleEndian) {
            BinaryPrimitives.WriteInt16LittleEndian(destination, value);
        }
        else {
            BinaryPrimitives.WriteInt16BigEndian(destination, value);
        }
        
    }
}