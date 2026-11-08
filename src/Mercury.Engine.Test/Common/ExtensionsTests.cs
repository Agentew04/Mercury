using Mercury.Engine.Common;

namespace Mercury.Engine.Test.Common;

[TestClass]
public class ExtensionsTests {
    [TestMethod]
    public void Short_12Bits_PositiveValue()
    {
        short value = 2047;

        Assert.AreEqual((short)2047, value.SignExtend(12));
    }

    [TestMethod]
    public void Short_12Bits_NegativeValue()
    {
        short value = 0xFFF;

        Assert.AreEqual((short)-1, value.SignExtend(12));
    }

    [TestMethod]
    public void Short_12Bits_MinNegativeValue()
    {
        short value = 0x800;

        Assert.AreEqual((short)-2048, value.SignExtend(12));
    }

    [TestMethod]
    public void Short_12Bits_MaxPositiveValue()
    {
        short value = 0x7FF;

        Assert.AreEqual((short)2047, value.SignExtend(12));
    }

    [TestMethod]
    public void Short_8Bits_NegativeValue()
    {
        short value = 0x80;

        Assert.AreEqual((short)-128, value.SignExtend(8));
    }

    [TestMethod]
    public void Short_8Bits_PositiveValue()
    {
        short value = 0x7F;

        Assert.AreEqual((short)127, value.SignExtend(8));
    }

    [TestMethod]
    public void Short_FullWidth_DoesNotChangeValue()
    {
        short value = -12345;

        Assert.AreEqual(value, value.SignExtend(16));
    }

    [TestMethod]
    public void Int_16Bits_NegativeValue()
    {
        int value = 0x8000;

        Assert.AreEqual(-32768, value.SignExtend(16));
    }

    [TestMethod]
    public void Int_16Bits_PositiveValue()
    {
        int value = 0x7FFF;

        Assert.AreEqual(32767, value.SignExtend(16));
    }

    [TestMethod]
    public void Int_12Bits_NegativeValue()
    {
        int value = 0x800;

        Assert.AreEqual(-2048, value.SignExtend(12));
    }

    [TestMethod]
    public void Long_32Bits_NegativeValue()
    {
        long value = 0x80000000;

        Assert.AreEqual(-2147483648L, value.SignExtend(32));
    }

    [TestMethod]
    public void Long_32Bits_PositiveValue()
    {
        long value = 0x7FFFFFFF;

        Assert.AreEqual(2147483647L, value.SignExtend(32));
    }

    [TestMethod]
    public void Byte_4Bits_NegativeValue()
    {
        // Requer que a implementação suporte tipos unsigned
        // com interpretação de sinal.
        byte value = 0xF;

        Assert.AreEqual(255, value.SignExtend(4));
        Assert.AreEqual(-1, (sbyte)value.SignExtend(4));
    }

    [TestMethod]
    public void Int_1Bit_NegativeValue()
    {
        int value = 1;

        Assert.AreEqual(-1, value.SignExtend(1));
    }

    [TestMethod]
    public void Int_1Bit_ZeroValue()
    {
        int value = 0;

        Assert.AreEqual(0, value.SignExtend(1));
    }
}