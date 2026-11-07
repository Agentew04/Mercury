using System.Numerics;
using Mercury.Engine.Common;
using Mercury.Engine.Memory;

namespace Mercury.Engine.Test.Common;

/*
 * Adaptado de arquivo gerado com IA.
 *
 * Usado p/ testar varias implementacoes do ReadBits
 * durante otimizacoes
 */

/// <summary>
/// Tests for <see cref="InstructionBits"/>.
/// <para>
/// The value tests are written against an independent <see cref="BigInteger"/>-based oracle
/// instead of repeating the production bit-loop, so a bug in the algorithm is not mirrored in
/// the expected values.
/// </para>
/// </summary>
[TestClass]
public class InstructionBitsTest {

    private static readonly byte[][] RepresentativePatterns = [
        [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00],
        [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF],
        [0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x11],
        [0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA],
        [0x80, 0x00, 0x40, 0x00, 0x20, 0x00, 0x10, 0x00, 0x08],
        [0x5D, 0xE3, 0x17, 0x8B, 0xC4, 0x29, 0xF6, 0x70, 0x3C],
    ];

    // ---------------------------------------------------------------------------------------
    // Known vectors
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void ReadBits_KnownVectors_LittleEndian() {
        byte[] bytes = [0x12, 0x34, 0x56, 0x78];

        AssertVector(bytes, 4, 0, 0, Endianness.LittleEndian, 0);
        AssertVector(bytes, 4, 1, 1, Endianness.LittleEndian, 1);
        AssertVector(bytes, 4, 0, 7, Endianness.LittleEndian, 0x12);
        AssertVector(bytes, 4, 8, 15, Endianness.LittleEndian, 0x34);
        AssertVector(bytes, 4, 16, 23, Endianness.LittleEndian, 0x56);
        AssertVector(bytes, 4, 24, 31, Endianness.LittleEndian, 0x78);
        AssertVector(bytes, 4, 0, 15, Endianness.LittleEndian, 0x3412);
        AssertVector(bytes, 4, 16, 31, Endianness.LittleEndian, 0x7856);
        AssertVector(bytes, 4, 0, 31, Endianness.LittleEndian, 0x78563412);

        // Unaligned fields crossing a byte boundary.
        AssertVector(bytes, 4, 4, 11, Endianness.LittleEndian, 0x41);
        AssertVector(bytes, 4, 7, 15, Endianness.LittleEndian, 0x68);
    }

    [TestMethod]
    public void ReadBits_KnownVectors_BigEndian() {
        byte[] bytes = [0x12, 0x34, 0x56, 0x78];

        AssertVector(bytes, 4, 0, 0, Endianness.BigEndian, 0);
        AssertVector(bytes, 4, 3, 3, Endianness.BigEndian, 1);
        AssertVector(bytes, 4, 0, 7, Endianness.BigEndian, 0x78);
        AssertVector(bytes, 4, 8, 15, Endianness.BigEndian, 0x56);
        AssertVector(bytes, 4, 16, 23, Endianness.BigEndian, 0x34);
        AssertVector(bytes, 4, 24, 31, Endianness.BigEndian, 0x12);
        AssertVector(bytes, 4, 0, 15, Endianness.BigEndian, 0x5678);
        AssertVector(bytes, 4, 16, 31, Endianness.BigEndian, 0x1234);
        AssertVector(bytes, 4, 0, 31, Endianness.BigEndian, 0x12345678);

        // Unaligned fields crossing a byte boundary.
        AssertVector(bytes, 4, 4, 11, Endianness.BigEndian, 0x67);
        AssertVector(bytes, 4, 7, 15, Endianness.BigEndian, 0xAC);
    }

    [TestMethod]
    public void ReadBits_KnownVectors_SingleByte() {
        byte[] bytes = [0xA5];

        AssertVector(bytes, 1, 0, 7, Endianness.LittleEndian, 0xA5);
        AssertVector(bytes, 1, 0, 7, Endianness.BigEndian, 0xA5);
        AssertVector(bytes, 1, 0, 3, Endianness.LittleEndian, 0x5);
        AssertVector(bytes, 1, 4, 7, Endianness.LittleEndian, 0xA);
        AssertVector(bytes, 1, 1, 4, Endianness.LittleEndian, 0b0010);
        AssertVector(bytes, 1, 7, 7, Endianness.LittleEndian, 1);
    }

    [TestMethod]
    public void ReadBits_FullWidthFields() {
        byte[] bytes = [0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF];

        AssertVector(bytes, 8, 0, 63, Endianness.LittleEndian, 0xEFCDAB8967452301);
        AssertVector(bytes, 8, 0, 63, Endianness.BigEndian, 0x0123456789ABCDEF);

        AssertVector(bytes, 8, 63, 63, Endianness.LittleEndian, 1);
        AssertVector(bytes, 8, 63, 63, Endianness.BigEndian, 0);
        AssertVector(bytes, 8, 0, 7, Endianness.LittleEndian, 0x01);
        AssertVector(bytes, 8, 0, 7, Endianness.BigEndian, 0xEF);
        AssertVector(bytes, 8, 56, 63, Endianness.LittleEndian, 0xEF);
        AssertVector(bytes, 8, 56, 63, Endianness.BigEndian, 0x01);
    }

    [TestMethod]
    public void ReadBits_FieldsInInstructionsWiderThan64Bits() {
        // 9-byte (72-bit) instruction: the field is still limited to 64 bits, but it may start
        // above bit 63 and the whole instruction must be addressable.
        byte[] bytes = [0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x11];

        AssertVector(bytes, 9, 8, 71, Endianness.LittleEndian, 0x11EFCDAB89674523);
        AssertVector(bytes, 9, 8, 71, Endianness.BigEndian, 0x0123456789ABCDEF);

        AssertVector(bytes, 9, 0, 7, Endianness.LittleEndian, 0x01);
        AssertVector(bytes, 9, 0, 7, Endianness.BigEndian, 0x11);
        AssertVector(bytes, 9, 64, 71, Endianness.LittleEndian, 0x11);
        AssertVector(bytes, 9, 64, 71, Endianness.BigEndian, 0x01);
    }

    [TestMethod]
    public void ReadBits_ReversedBitRangeIsNormalised() {
        byte[] bytes = [0x12, 0x34, 0x56, 0x78];

        AssertVector(bytes, 4, 11, 4, Endianness.LittleEndian, 0x41);
        AssertVector(bytes, 4, 15, 7, Endianness.LittleEndian, 0x68);
        AssertVector(bytes, 4, 31, 0, Endianness.LittleEndian, 0x78563412);

        AssertVector(bytes, 4, 11, 4, Endianness.BigEndian, 0x67);
        AssertVector(bytes, 4, 15, 7, Endianness.BigEndian, 0xAC);
        AssertVector(bytes, 4, 31, 0, Endianness.BigEndian, 0x12345678);
    }

    [TestMethod]
    public void ReadBits_OnlyUsesBytesUpToByteCount() {
        // The trailing bytes must be ignored: for big-endian instructions the least significant
        // byte is the last byte *inside byteCount*, not the last byte of the span.
        byte[] bytes = [0x12, 0x34, 0xDE, 0xAD, 0xBE, 0xEF, 0xFF, 0xFF];

        AssertVector(bytes, 2, 0, 15, Endianness.LittleEndian, 0x3412);
        AssertVector(bytes, 2, 0, 15, Endianness.BigEndian, 0x1234);
        AssertVector(bytes, 2, 4, 11, Endianness.LittleEndian, 0x41);
        AssertVector(bytes, 2, 4, 11, Endianness.BigEndian, 0x23);
    }

    [TestMethod]
    public void ReadBits_SingleBitFieldsAtEveryPosition() {
        byte[] bytes = [0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x11];

        for (int bit = 0; bit < bytes.Length * 8; bit++) {
            AssertMatchesOracle(bytes, bytes.Length, bit, bit, Endianness.LittleEndian);
            AssertMatchesOracle(bytes, bytes.Length, bit, bit, Endianness.BigEndian);
        }
    }

    // ---------------------------------------------------------------------------------------
    // Oracle-based exhaustive / randomized checks
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void ReadBits_MatchesOracle_ExhaustivelyForRepresentativePatterns() {
        for (int byteCount = 1; byteCount <= 9; byteCount++) {
            foreach (byte[] pattern in RepresentativePatterns) {
                ReadOnlySpan<byte> bytes = pattern.AsSpan(0, byteCount);
                int totalBits = byteCount * 8;

                for (int bitStart = 0; bitStart < totalBits; bitStart++) {
                    // A single field can never be wider than 64 bits.
                    int lastEnd = Math.Min(totalBits - 1, bitStart + 63);
                    for (int bitEnd = bitStart; bitEnd <= lastEnd; bitEnd++) {
                        AssertMatchesOracle(bytes, byteCount, bitStart, bitEnd, Endianness.LittleEndian);
                        AssertMatchesOracle(bytes, byteCount, bitStart, bitEnd, Endianness.BigEndian);
                    }
                }
            }
        }
    }

    [TestMethod]
    public void ReadBits_MatchesOracle_Randomized() {
        Random random = new(0x5EED);
        byte[] bytes = new byte[24];

        for (int iteration = 0; iteration < 5_000; iteration++) {
            int byteCount = random.Next(1, bytes.Length + 1);
            random.NextBytes(bytes);

            int totalBits = byteCount * 8;
            int bitStart = random.Next(totalBits);
            int width = random.Next(1, Math.Min(64, totalBits - bitStart) + 1);
            int bitEnd = bitStart + width - 1;
            Endianness endianness = random.Next(2) == 0
                ? Endianness.LittleEndian
                : Endianness.BigEndian;

            AssertMatchesOracle(bytes, byteCount, bitStart, bitEnd, endianness);

            // Reversed spans must be normalised to the same field.
            AssertMatchesOracle(bytes, byteCount, bitEnd, bitStart, endianness);
        }
    }

    [TestMethod]
    public void ReadBits_MatchesOracle_WithBytesPastByteCount() {
        Random random = new(0xC0FFEE);
        byte[] bytes = new byte[32];

        for (int iteration = 0; iteration < 2_000; iteration++) {
            int byteCount = random.Next(1, bytes.Length);
            random.NextBytes(bytes);

            int totalBits = byteCount * 8;
            int bitStart = random.Next(totalBits);
            int bitEnd = random.Next(bitStart, Math.Min(totalBits - 1, bitStart + 63) + 1);

            AssertMatchesOracle(bytes, byteCount, bitStart, bitEnd, Endianness.LittleEndian);
            AssertMatchesOracle(bytes, byteCount, bitStart, bitEnd, Endianness.BigEndian);
        }
    }

    // ---------------------------------------------------------------------------------------
    // Argument validation
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void ReadBits_RejectsZeroOrNegativeByteCount() {
        byte[] bytes = new byte[16];
        Assert.Throws<ArgumentOutOfRangeException>(
            () => InstructionBits.ReadBits(bytes, 0, 0, 0, Endianness.LittleEndian),
            "byteCount = 0");
        Assert.Throws<ArgumentOutOfRangeException>(
            () => InstructionBits.ReadBits(bytes, -1, 0, 0, Endianness.LittleEndian),
            "byteCount = -1");
    }

    [TestMethod]
    public void ReadBits_RejectsFieldsWiderThan64Bits() {
        byte[] bytes = new byte[16];
        
        Assert.Throws<ArgumentOutOfRangeException>(
            () => InstructionBits.ReadBits(bytes, 16, 0, 64, Endianness.LittleEndian),
            "65-bit field");
        Assert.Throws<ArgumentOutOfRangeException>(
            () => InstructionBits.ReadBits(bytes, 9, 8, 72, Endianness.BigEndian),
            "65-bit field starting above bit 63");
        // Reversed parameters are normalised before the width check.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => InstructionBits.ReadBits(bytes, 16, 64, 0, Endianness.LittleEndian),
            "reversed 65-bit field");
    }

    [TestMethod]
    public void ReadBits_RejectsBitsOutsideTheInstruction() {
        byte[] bytes = new byte[16];

        Assert.Throws<ArgumentOutOfRangeException>(
            () => InstructionBits.ReadBits(bytes, 4, 0, 32, Endianness.LittleEndian),
            "bitEnd == byteCount * 8");
        Assert.Throws<ArgumentOutOfRangeException>(
            () => InstructionBits.ReadBits(bytes, 4, 31, 40, Endianness.LittleEndian),
            "field ending past the instruction");
    }

    [TestMethod]
    public void ReadBits_RejectsSpanShorterThanByteCount() {
        Assert.Throws<ArgumentException>(
            () => InstructionBits.ReadBits(new byte[2], 3, 0, 7, Endianness.LittleEndian),
            "span shorter than byteCount");
    }

    // ---------------------------------------------------------------------------------------
    // WriteBits (kept alongside ReadBits so the two stay symmetric)
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void WriteBits_MatchesOracle() {
        Random random = new(unchecked((int)0xB16B00B5));
        byte[] buffer = new byte[16];

        for (int iteration = 0; iteration < 2_000; iteration++) {
            int byteCount = random.Next(1, buffer.Length + 1);
            int totalBits = byteCount * 8;
            int bitStart = random.Next(totalBits);
            int width = random.Next(1, Math.Min(64, totalBits - bitStart) + 1);
            int bitEnd = bitStart + width - 1;
            ulong value = NextUInt64(random);
            Endianness endianness = random.Next(2) == 0
                ? Endianness.LittleEndian
                : Endianness.BigEndian;

            byte[] zeroed = new byte[byteCount];
            byte[] actual = new byte[byteCount];
            InstructionBits.WriteBits(actual, byteCount, bitStart, bitEnd, value, endianness);
            byte[] expected = ReferenceWriteBits(zeroed, byteCount, bitStart, bitEnd, value, endianness);

            CollectionAssert.AreEqual(expected, actual,
                $"WriteBits disagreed with the oracle (byteCount={byteCount}, bits=[{bitStart}..{bitEnd}], endianness={endianness}).");
        }
    }

    [TestMethod]
    public void WriteBits_ThenReadBits_RoundTrips() {
        Random random = new(0xA11CE);
        byte[] buffer = new byte[16];

        for (int iteration = 0; iteration < 2_000; iteration++) {
            int byteCount = random.Next(1, buffer.Length + 1);
            int totalBits = byteCount * 8;
            int bitStart = random.Next(totalBits);
            int width = random.Next(1, Math.Min(64, totalBits - bitStart) + 1);
            int bitEnd = bitStart + width - 1;
            ulong value = NextUInt64(random);
            Endianness endianness = random.Next(2) == 0
                ? Endianness.LittleEndian
                : Endianness.BigEndian;

            Array.Clear(buffer);
            InstructionBits.WriteBits(buffer, byteCount, bitStart, bitEnd, value, endianness);
            ulong actual = InstructionBits.ReadBits(buffer, byteCount, bitStart, bitEnd, endianness);

            ulong expected = width == 64 ? value : value & ((1UL << width) - 1);
            Assert.AreEqual(expected, actual,
                $"Round-trip failed (byteCount={byteCount}, bits=[{bitStart}..{bitEnd}], endianness={endianness}).");
        }
    }

    [TestMethod]
    public void WriteBits_DoesNotTouchBitsOutsideTheField() {
        // Field [4..11] in a 3-byte instruction.
        byte[] littleEndian = new byte[3];
        InstructionBits.WriteBits(littleEndian, 3, 4, 11, 0xFF, Endianness.LittleEndian);
        CollectionAssert.AreEqual(new byte[] { 0xF0, 0x0F, 0x00 }, littleEndian);

        byte[] bigEndian = new byte[3];
        InstructionBits.WriteBits(bigEndian, 3, 4, 11, 0xFF, Endianness.BigEndian);
        CollectionAssert.AreEqual(new byte[] { 0x00, 0x0F, 0xF0 }, bigEndian);
    }

    // ---------------------------------------------------------------------------------------
    // Independent reference implementation (deliberately not the production bit loop)
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Builds the integer represented by the first <paramref name="byteCount"/> bytes, honouring
    /// endianness, using <see cref="BigInteger"/> so that instructions of any width are supported.
    /// </summary>
    private static BigInteger ToInteger(ReadOnlySpan<byte> bytes, int byteCount, Endianness endianness) {
        BigInteger value = BigInteger.Zero;
        for (int i = 0; i < byteCount; i++) {
            int significance = endianness == Endianness.LittleEndian ? i : byteCount - 1 - i;
            value += new BigInteger((int)bytes[i]) << (significance * 8);
        }
        return value;
    }

    private static ulong ReferenceReadBits(ReadOnlySpan<byte> bytes, int byteCount, int bitStart, int bitEnd, Endianness endianness) {
        if (bitEnd < bitStart) {
            (bitStart, bitEnd) = (bitEnd, bitStart);
        }
        int width = bitEnd - bitStart + 1;
        BigInteger mask = (BigInteger.One << width) - 1;
        BigInteger value = ToInteger(bytes, byteCount, endianness);
        return (ulong)((value >> bitStart) & mask);
    }

    private static byte[] ReferenceWriteBits(ReadOnlySpan<byte> original, int byteCount, int bitStart, int bitEnd,
        ulong value, Endianness endianness) {
        if (bitEnd < bitStart) {
            (bitStart, bitEnd) = (bitEnd, bitStart);
        }
        int width = bitEnd - bitStart + 1;
        BigInteger widthMask = (BigInteger.One << width) - 1;
        BigInteger fieldMask = widthMask << bitStart;

        BigInteger integer = ToInteger(original, byteCount, endianness);
        integer = (integer & ~fieldMask) | (((BigInteger)value & widthMask) << bitStart);

        byte[] result = new byte[byteCount];
        for (int i = 0; i < byteCount; i++) {
            int significance = endianness == Endianness.LittleEndian ? i : byteCount - 1 - i;
            result[i] = (byte)((integer >> (significance * 8)) & 0xFF);
        }
        return result;
    }

    // ---------------------------------------------------------------------------------------
    // Assertion helpers
    // ---------------------------------------------------------------------------------------

    private static void AssertVector(ReadOnlySpan<byte> bytes, int byteCount, int bitStart, int bitEnd,
        Endianness endianness, ulong expected) {
        ulong oracle = ReferenceReadBits(bytes, byteCount, bitStart, bitEnd, endianness);
        Assert.AreEqual(expected, oracle,
            $"The test oracle disagrees with the hard-coded expectation for bits=[{bitStart}..{bitEnd}], endianness={endianness}.");

        AssertAllImplementations(bytes, byteCount, bitStart, bitEnd, endianness, expected);
    }

    private static void AssertMatchesOracle(ReadOnlySpan<byte> bytes, int byteCount, int bitStart, int bitEnd,
        Endianness endianness) {
        ulong expected = ReferenceReadBits(bytes, byteCount, bitStart, bitEnd, endianness);
        AssertAllImplementations(bytes, byteCount, bitStart, bitEnd, endianness, expected);
    }

    private static void AssertAllImplementations(ReadOnlySpan<byte> bytes, int byteCount, int bitStart, int bitEnd,
        Endianness endianness, ulong expected) {
        ulong actual = InstructionBits.ReadBits(bytes, byteCount, bitStart, bitEnd, endianness);
        if (actual == expected) {
            return;
        }

        int low = Math.Min(bitStart, bitEnd);
        int high = Math.Max(bitStart, bitEnd);
        Assert.Fail("returned 0x{actual:X} but expected 0x{expected:X} " +
                    $"(bytes=0x{Convert.ToHexString(bytes[..byteCount])}, byteCount={byteCount}, " +
                    $"bits=[{low}..{high}], endianness={endianness}).");
    }

    private static ulong NextUInt64(Random random) {
        return (ulong)random.NextInt64() | ((ulong)random.Next(2) << 63);
    }
}
