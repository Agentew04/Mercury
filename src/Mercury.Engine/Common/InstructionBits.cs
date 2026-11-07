using System.Runtime.CompilerServices;
using Mercury.Engine.Memory;

namespace Mercury.Engine.Common;

/// <summary>
/// Endian-aware bit-packing helper used by generated instruction code.
/// Bit 0 is the least significant bit of the integer formed by the bytes, preserving
/// ISA-manual bit numbering. It supports instructions of any width; only an individual
/// field is limited to 64 bits, since that is the largest value a single C# field type
/// can hold.
/// </summary>
public static class InstructionBits {

    /// <summary>
    /// Reads a bit span <c>[bitStart, bitEnd]</c> (inclusive) from a byte slice and returns it as an
    /// unsigned 64-bit value. Bit 0 is the least significant bit of the integer formed by the bytes.
    /// </summary>
    /// <param name="bytes">The span with the bytes to read from</param>
    /// <param name="byteCount">The number of bytes of the instruction, starting at the first byte</param>
    /// <param name="bitStart">The inclusive index of the first bit</param>
    /// <param name="bitEnd">The inclusive index of the last bit</param>
    /// <param name="endianness">The endianness of the byte sequence</param>
    /// <returns>The value of the bit span</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the bit span is wider than 64 bits or lies outside the instruction</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="bytes"/> has fewer than <paramref name="byteCount"/> bytes</exception>
    public static ulong ReadBits(ReadOnlySpan<byte> bytes, int byteCount, int bitStart, int bitEnd, Endianness endianness) {
        if (bitEnd < bitStart) {
            (bitStart, bitEnd) = (bitEnd, bitStart);
        }
        ValidateRange(bytes, byteCount, bitStart, bitEnd);
        return ReadBitsFast(bytes, byteCount, bitStart, bitEnd, endianness);
    }

    internal static ulong ReadBitsFast(ReadOnlySpan<byte> bytes, int byteCount, int bitStart, int bitEnd,
        Endianness endianness) {
        if (bitEnd < bitStart) {
            (bitStart, bitEnd) = (bitEnd, bitStart);
        }

        int remaining = bitEnd - bitStart + 1;
        int sourceBit = bitStart;
        int destinationShift = 0;

        ulong result = 0;

        while (remaining > 0) {
            int bitOffset = sourceBit & 7;
            int count = Math.Min(8 - bitOffset, remaining);

            int byteIndex = endianness == Endianness.LittleEndian
                ? sourceBit >> 3
                : byteCount - 1 - (sourceBit >> 3);

            ulong mask = (1UL << count) - 1;
            ulong chunk = ((ulong)bytes[byteIndex] >> bitOffset) & mask;

            result |= chunk << destinationShift;

            sourceBit += count;
            destinationShift += count;
            remaining -= count;
        }

        return result;
    }
    
    /// <summary>
    /// Writes the low bits of <paramref name="value"/> into the bit span <c>[bitStart, bitEnd]</c>
    /// (inclusive) of a byte slice. Only bits within the span are written; the buffer is expected
    /// to be cleared beforehand (e.g. by <see cref="ToBytes"/> callers). Bit 0 is the least
    /// significant bit of the integer formed by the bytes.
    /// </summary>
    /// <param name="bytes">The span to write the bytes to</param>
    /// <param name="byteCount">The number of bytes of the instruction, starting at the first byte</param>
    /// <param name="bitStart">The inclusive index of the first bit</param>
    /// <param name="bitEnd">The inclusive index of the last bit</param>
    /// <param name="value">The value to write</param>
    /// <param name="endianness">The endianness of the byte sequence</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the bit span is wider than 64 bits or lies outside the instruction</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="bytes"/> has fewer than <paramref name="byteCount"/> bytes</exception>
    public static void WriteBits(Span<byte> bytes, int byteCount, int bitStart, int bitEnd, ulong value, Endianness endianness) {
        if (bitEnd < bitStart) {
            (bitStart, bitEnd) = (bitEnd, bitStart);
        }

        ValidateRange(bytes, byteCount, bitStart, bitEnd);
        WriteBitsFast(bytes, byteCount, bitStart, bitEnd, value, endianness);
    }

    internal static void WriteBitsFast(Span<byte> bytes, int byteCount, int bitStart, int bitEnd, ulong value, Endianness endianness) {
        if (bitEnd < bitStart) {
            (bitStart, bitEnd) = (bitEnd, bitStart);
        }

        int remaining = bitEnd - bitStart + 1;
        int destinationBit = bitStart;
        int sourceShift = 0;

        while (remaining > 0) {
            int bitOffset = destinationBit & 7;
            int count = Math.Min(8 - bitOffset, remaining);

            int byteIndex = endianness == Endianness.LittleEndian
                ? destinationBit >> 3
                : byteCount - 1 - (destinationBit >> 3);

            ulong mask = (1UL << count) - 1;
            ulong chunk = (value >> sourceShift) & mask;

            bytes[byteIndex] |= (byte)(chunk << bitOffset);

            destinationBit += count;
            sourceShift += count;
            remaining -= count;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateRange(
        ReadOnlySpan<byte> bytes,
        int byteCount,
        int bitStart,
        int bitEnd) {

        if (byteCount <= 0) {
            throw new ArgumentOutOfRangeException(nameof(byteCount));
        }

        if (bytes.Length < byteCount) {
            throw new ArgumentException(
                $"The span must have at least {byteCount} bytes.",
                nameof(bytes));
        }

        if (bitStart < 0) {
            throw new ArgumentOutOfRangeException(nameof(bitStart));
        }

        long bitCount = (long)bitEnd - bitStart + 1;

        if (bitCount > 64) {
            throw new ArgumentOutOfRangeException(nameof(bitEnd));
        }

        if (bitEnd >= (long)byteCount * 8) {
            throw new ArgumentOutOfRangeException(nameof(bitEnd));
        }
    }
}