using System.Text.RegularExpressions;
using Mercury.Engine.Common;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Test.Mips;

[TestClass]
public class TypeJTest {

    public const Endianness BigEndian = Endianness.BigEndian;
    
    [TestCategory("J")]
    [DataRow(0x0040001C, 0x08100007U)]
    [DataRow(0x00400018, 0x08100006U)]
    [TestMethod]
    public void JAssembly(int address, uint expected) {
        var instruction = new J {
            Immediate = (address&0x3FFFFFF)>>2
        };
        Assert.AreEqual(expected, instruction.ConvertToInt(Endianness.BigEndian));
    }

    [TestCategory("J")]
    [DataRow(0x08100007U)]
    [DataRow(0x08100006U)]
    [TestMethod]
    public void JDisassembly(uint instructionInt) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(instructionInt, BigEndian), new InstructionPool());
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<J>(instruction);
        Assert.AreEqual(instructionInt, instruction.ConvertToInt(Endianness.BigEndian));
    }

    [TestCategory("Jal")]
    [DataRow(0x0040001C, 0x0C100007U)]
    [DataRow(0x00400018, 0x0C100006U)]
    [TestMethod]
    public void JalAssembly(int address, uint expected) {
        Jal instruction = new() {
            Immediate = (address&0x3FFFFFF)>>2
        };
        Assert.AreEqual(expected, instruction.ConvertToInt(Endianness.BigEndian));
    }
    
    [TestCategory("Jal")]
    [DataRow(0x0C100007U)]
    [DataRow(0x0C100006U)]
    [TestMethod]
    public void JalDisassembly(uint instructionInt) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(instructionInt, BigEndian), new InstructionPool());
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Jal>(instruction);
        Assert.AreEqual(instructionInt, instruction.ConvertToInt(Endianness.BigEndian));
    }
}
