using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Mercury.Engine.Memory;

namespace Mercury.Engine.Common;

public ref struct InstructionBitSource {
    public int BitPosition { get; private set; }
    private readonly Span<byte> buffer;
    private readonly Endianness endianness;
    public InstructionBitSource(Span<byte> bytes, Endianness endianness) {
        BitPosition = 0;
        buffer = bytes;
        this.endianness = endianness;
    }

    public T GetBits<T>(int count) where T : IBinaryInteger<T> {
        foreach (byte b in buffer) {
            Console.Write(b.ToString("B8"));
            Console.Write(" ");
        }
        Console.WriteLine();
        int dataSize = Unsafe.SizeOf<T>() * 8; 
        if (count > dataSize || count <= 0) {
            throw new ArgumentOutOfRangeException(nameof(count), "Invalid bit count size.");
        }
        if (count > buffer.Length * 8) {
            throw new ArgumentException(nameof(count));
        }
        
        int firstByte = BitPosition / 8;
        int lastByte = (BitPosition + count-1) / 8;
        T result = T.Zero;
        Span<byte> slice = stackalloc byte[lastByte-firstByte+1];
        ReorderSlice(slice, buffer, firstByte, lastByte, endianness);
        
        // Span<byte> slice = buffer[firstByte..(lastByte+1)];
        // Console.WriteLine($"Buffer: {buffer.Length*8}; Pos: {BitPosition}; Count: {count}; First: {firstByte}/{BitPosition}; Last: {lastByte}/{BitPosition+count-1}");

        int bitsAvailable = 8 - (BitPosition % 8);
        int currentByte = 0;

        int read = 0;
        while (read < count) {
            int remaining = count - read;
            int toRead = Math.Min(Math.Min(8, remaining), bitsAvailable);
            
            // extract
            int positionInByte = BitPosition % 8;
            byte current = slice[currentByte];
            // Console.WriteLine($"Read; ToRead: {toRead}; PosInByte: {positionInByte};");

            byte mask = (byte)(0xFF >> positionInByte);
            // Console.WriteLine($"mask: {mask:B8}");
            // Console.WriteLine($"read: {(current & mask):B8}");
            // Console.WriteLine($"read: {((current & mask)>> (8-positionInByte-toRead)):B8}");
            
            // put on T result
            result <<= toRead;
            result |= T.CreateChecked((current & mask) >> (8-positionInByte-toRead));
            // Console.WriteLine(result.ToString("B32", new NumberFormatInfo()));
            

            read += toRead;
            BitPosition += toRead;
            currentByte++;
        }
        return result;
    }

    private static void ReorderSlice(Span<byte> slice, Span<byte> buffer, int firstByte, int lastByte, Endianness endianness) {
        // on little endian
        // 0 -> 3
        // 1 -> 2
        // 2 -> 1
        // 3 -> 0
        int current = 0;
        if (endianness == Endianness.BigEndian) {
            for (int i = firstByte; i <= lastByte; i++, current++) {
                slice[current] = buffer[i];
            }
        }
        else {
            // little
            for (int i = firstByte; i <= lastByte; i++, current++) {
                int onWord = i % 4;
                int word = i / 4;
                int newOnWord = 3 - onWord;
                Console.WriteLine($"i: {i} word: {word} onWord: {onWord} new: {newOnWord}");
                slice[current] = buffer[word+newOnWord];
            }
        }
    }
}