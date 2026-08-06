using System.Buffers.Binary;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory;

namespace Mercury.Engine.RiscV.RV32I.Runtime;

public partial class Monocycle {
    private void ReadMemory(ulong address, Memory<byte> buffer) {
        MemoryReadEvent read = new() {
            Address = address,
            Buffer = buffer,
            Size = (ulong)buffer.Length
        };
        eventBus.Publish(read);
    }
    
    private int BytesToInt32(ReadOnlySpan<byte> word) {
        return endianess == Endianess.LittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(word) : BinaryPrimitives.ReadInt32BigEndian(word);
    }

    private short BytesToInt16(ReadOnlySpan<byte> word) {
        return endianess == Endianess.LittleEndian ? BinaryPrimitives.ReadInt16LittleEndian(word) : BinaryPrimitives.ReadInt16BigEndian(word);
    }

    private void Int32ToBytes(int value, Span<byte> destination) {
        if (endianess == Endianess.LittleEndian) {
            BinaryPrimitives.WriteInt32LittleEndian(destination, value);
        }
        else {
            BinaryPrimitives.WriteInt32BigEndian(destination, value);
        }
    }

    private void Int16ToBytes(short value, Span<byte> destination) {
        if (endianess == Endianess.LittleEndian) {
            BinaryPrimitives.WriteInt16LittleEndian(destination, value);
        }
        else {
            BinaryPrimitives.WriteInt16BigEndian(destination, value);
        }
        
    }
}