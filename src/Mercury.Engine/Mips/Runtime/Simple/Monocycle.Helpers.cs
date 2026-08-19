using System.Buffers.Binary;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle {
    private static bool IsOverflowed(int a, int b, int result) {
        return (a > 0 && b > 0 && result < 0) || (a < 0 && b < 0 && result > 0);
    }

    private void BranchTo(int immediate) {
        isExecutingBranch = true;
        branchAddress = (uint)(Registers.Get(MipsGprRegisters.Pc) + 4 + (immediate << 2));
    }
    private void Link(MipsGprRegisters register = MipsGprRegisters.Ra) {
        Registers.Set(register, Registers.Get(MipsGprRegisters.Pc) + (UseBranchDelaySlot ? 8 : 4));
    }

    private static int ZeroExtend(short value) {
        return (ushort)value;
    }
    
    private void ReadMemory(ulong address, Memory<byte> buffer) {
        MemoryReadEvent read = new() {
            Address = address,
            Buffer = buffer,
            Size = (ulong)buffer.Length
        };
        eventBus.Publish(read);
    }
    
    private void WriteMemory(ulong address, Memory<byte> buffer) {
        MemoryWriteEvent write = new() {
            Address = address,
            Buffer = buffer,
            Size = (ulong)buffer.Length
        };
        eventBus.Publish(write);
    }

    private int BytesToInt32(ReadOnlySpan<byte> word) {
        return Endianess == Endianess.LittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(word) : BinaryPrimitives.ReadInt32BigEndian(word);
    }

    private short BytesToInt16(ReadOnlySpan<byte> word) {
        return Endianess == Endianess.LittleEndian ? BinaryPrimitives.ReadInt16LittleEndian(word) : BinaryPrimitives.ReadInt16BigEndian(word);
    }

    private void Int32ToBytes(int value, Span<byte> destination) {
        if (Endianess == Endianess.LittleEndian) {
            BinaryPrimitives.WriteInt32LittleEndian(destination, value);
        }
        else {
            BinaryPrimitives.WriteInt32BigEndian(destination, value);
        }
    }

    private void Int16ToBytes(short value, Span<byte> destination) {
        if (Endianess == Endianess.LittleEndian) {
            BinaryPrimitives.WriteInt16LittleEndian(destination, value);
        }
        else {
            BinaryPrimitives.WriteInt16BigEndian(destination, value);
        }
        
    }
}