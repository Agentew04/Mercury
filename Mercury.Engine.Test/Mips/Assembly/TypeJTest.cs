using System.Text.RegularExpressions;
using Mercury.Engine.Common;
using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Test.Mips;

[TestClass]
public class TypeJTest {

    [TestCategory("J")]
    [DataRow(0x0040001C, 0x08100007U)]
    [DataRow(0x00400018, 0x08100006U)]
    [TestMethod]
    public void JAssembly(int address, uint expected) {
        var instruction = new J {
            Immediate = (address&0x3FFFFFF)>>2
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("J")]
    [DataRow(0x08100007U)]
    [DataRow(0x08100006U)]
    [TestMethod]
    public void JDisassembly(uint instructionInt) {
        IInstruction? instruction = Disassembler.Disassemble(instructionInt);
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<J>(instruction);
        Assert.AreEqual(instructionInt, instruction.ConvertToInt());
    }

    [TestCategory("Jal")]
    [DataRow(0x0040001C, 0x0C100007U)]
    [DataRow(0x00400018, 0x0C100006U)]
    [TestMethod]
    public void JalAssembly(int address, uint expected) {
        var instruction = new Jal {
            Immediate = (address&0x3FFFFFF)>>2
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }
    
    [TestCategory("Jal")]
    [DataRow(0x0C100007U)]
    [DataRow(0x0C100006U)]
    [TestMethod]
    public void JalDisassembly(uint instructionInt) {
        IInstruction? instruction = Disassembler.Disassemble(instructionInt);
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Jal>(instruction);
        Assert.AreEqual(instructionInt, instruction.ConvertToInt());
    }
}
