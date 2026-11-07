using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory;

namespace Mercury.Engine.Common;

/// <summary>
/// <see cref="IInstructionByteSource"/> that pulls instruction bytes from the memory
/// module through the event bus. Bytes are cached in blocks and fetched lazily, so the
/// decoder only causes fetches when it actually needs more data.
/// </summary>
public class MemoryByteSource : IInstructionByteSource {

    private const int BufferSize = 64;
    private readonly EventBus bus;

    private readonly Memory<byte> buffer = new byte[BufferSize];
    private ulong bufferStart;
    private ulong virtualStart;
    
    /// <summary>
    /// Creates a source that reads from memory through the given event bus.
    /// </summary>
    /// <param name="bus">The event bus of the machine</param>
    /// <param name="endianness">The endianness of the memory being read</param>
    public MemoryByteSource(EventBus bus, Endianness endianness) {
        this.bus = bus;
        Endianness = endianness;
    }

    public Endianness Endianness { get; }

    /// <summary>
    /// Points the stream start (offset 0) to the given address.
    /// </summary>
    /// <param name="address">The absolute address of the first byte of the stream</param>
    public void StartAt(ulong address) {
        virtualStart = address;
    }

    public bool TryRead(long offset, out byte value) {
        ulong address = virtualStart + (ulong)offset;
        long bufferOffset = (long)(address - bufferStart);
        if (bufferOffset is < 0 or >= BufferSize) {
            // refetch
            bufferStart = address;
            try {
                bus.Publish(new MemoryReadEvent() {
                    Address = bufferStart,
                    Buffer = buffer,
                    Size = BufferSize
                });
                bufferOffset = (long)(address - bufferStart);
            }
            catch (InvalidAddressException) {
                // read out of the addressable region
                value = 0;
                return false;
            }
        }
        value = buffer.Span[(int)bufferOffset];
        return true;
    }
}