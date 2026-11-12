using Mercury.Engine.Common;
using Mercury.Engine.Memory;

namespace Mercury.Engine.Test.Common;

[TestClass]
public class InstructionBitSourceTests {
 
    [TestMethod]
    public void Constructor_StartsAtBitZero()
    {
        Span<byte> bytes = [0xFF];
        var source = new InstructionBitSource(
            bytes, Endianness.LittleEndian);

        Assert.AreEqual(0, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsEntireByte()
    {
        Span<byte> bytes = [0xAB];
        var source = new InstructionBitSource(
            bytes, Endianness.LittleEndian);

        byte result = source.GetBits<byte>(8);

        Assert.AreEqual(0xAB, result);
        Assert.AreEqual(8, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsPartialByte_LittleEndian()
    {
        Span<byte> bytes = [0b1011_0010];
        var source = new InstructionBitSource(
            bytes, Endianness.LittleEndian);

        byte result = source.GetBits<byte>(4);

        Assert.AreEqual(0b0010, result);
        Assert.AreEqual(4, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsPartialByte_BigEndian()
    {
        Span<byte> bytes = [0b1011_0010];
        var source = new InstructionBitSource(
            bytes, Endianness.BigEndian);

        byte result = source.GetBits<byte>(4);

        Assert.AreEqual(0b1011, result);
        Assert.AreEqual(4, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsRemainingBitsOfByte()
    {
        Span<byte> bytes = [0b1011_0010];
        var source = new InstructionBitSource(
            bytes, Endianness.LittleEndian);

        _ = source.GetBits<byte>(4);
        byte result = source.GetBits<byte>(4);

        Assert.AreEqual(0b1011, result);
        Assert.AreEqual(8, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsAcrossBytes_LittleEndian()
    {
        Span<byte> bytes = [0xCD, 0xAB];
        var source = new InstructionBitSource(
            bytes, Endianness.LittleEndian);

        ushort result = source.GetBits<ushort>(12);

        Assert.AreEqual(0xBCD, result);
        Assert.AreEqual(12, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsAcrossBytes_BigEndian()
    {
        Span<byte> bytes = [0xCD, 0xAB];
        var source = new InstructionBitSource(
            bytes, Endianness.BigEndian);

        ushort result = source.GetBits<ushort>(12);

        Assert.AreEqual(0xCDA, result);
        Assert.AreEqual(12, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsUnalignedBits_LittleEndian()
    {
        Span<byte> bytes = [0b1011_0110, 0b0110_1001];
        var source = new InstructionBitSource(
            bytes, Endianness.LittleEndian);

        _ = source.GetBits<byte>(5);
        byte result = source.GetBits<byte>(5);

        Assert.AreEqual(0b01101, result);
        Assert.AreEqual(10, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsUnalignedBits_BigEndian()
    {
        Span<byte> bytes = [0b1011_0110, 0b0110_1001];
        var source = new InstructionBitSource(
            bytes, Endianness.BigEndian);

        _ = source.GetBits<byte>(5);
        byte result = source.GetBits<byte>(5);

        Assert.AreEqual(0b11001, result);
        Assert.AreEqual(10, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsMultipleSequentialFields()
    {
        Span<byte> bytes = [0b1101_1010, 0b1011_0101];
        var source = new InstructionBitSource(
            bytes, Endianness.LittleEndian);

        byte first = source.GetBits<byte>(4);
        byte second = source.GetBits<byte>(4);
        byte third = source.GetBits<byte>(8);

        Assert.AreEqual(0b1010, first);
        Assert.AreEqual(0b1101, second);
        Assert.AreEqual(0b1011_0101, third);
        Assert.AreEqual(16, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsMaximumByteWidth()
    {
        Span<byte> bytes = [0xFF, 0xFF, 0xFF, 0xFF];
        var source = new InstructionBitSource(
            bytes, Endianness.LittleEndian);

        uint result = source.GetBits<uint>(32);

        Assert.AreEqual(uint.MaxValue, result);
        Assert.AreEqual(32, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_ReadsPartialUnsignedValueLittle()
    {
        Span<byte> bytes = [0b1111_1111];
        var source = new InstructionBitSource(
            bytes, Endianness.LittleEndian);

        byte result = source.GetBits<byte>(3);

        Assert.AreEqual(7, result);
    }
    
    [TestMethod]
    public void GetBits_ReadsPartialUnsignedValueBig()
    {
        Span<byte> bytes = [0b1111_1111];
        var source = new InstructionBitSource(
            bytes, Endianness.BigEndian);

        byte result = source.GetBits<byte>(3);

        Assert.AreEqual(7, result);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(9)]
    public void GetBits_InvalidCount_Throws(int count)
    {
        Span<byte> bytes = [0xFF];
        InstructionBitSource source = new(bytes, Endianness.LittleEndian);

        bool throws = false;
        try {
            source.GetBits<byte>(count);
        }
        catch (ArgumentOutOfRangeException) {
            throws = true;
        }
        Assert.IsTrue(throws, "Method should have thrown ArgumentOutOfRangeException");
        Assert.AreEqual(0, source.BitPosition);
    }

    [TestMethod]
    public void GetBits_InsufficientBuffer_Throws()
    {
        Span<byte> bytes = [0xFF];
        var source = new InstructionBitSource(
            bytes, Endianness.LittleEndian);

        bool throws = false;
        try {
            source.GetBits<ushort>(16);
        }
        catch (ArgumentException) {
            throws = true;
        }
        Assert.IsTrue(throws, "Method should have thrown ArgumentException");
        Assert.AreEqual(0, source.BitPosition);
    }
   
}