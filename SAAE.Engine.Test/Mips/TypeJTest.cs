using SAAE.Engine.Mips.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAAE.Engine.Test.Mips;

[TestClass]
public class TypeJTest {

    [TestCategory("J")]
    [TestMethod]
    public void JRegex() {
        var instruction = new J();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("j 0x00400000"));
        Assert.IsTrue(regex.IsMatch("j 0x00400004"));
        Assert.IsFalse(regex.IsMatch("jal 0x0040000"));
    }

    [TestCategory("J")]
    [DataRow(0x0040001C, 0x08100007)]
    [DataRow(0x00400018, 0x08100006)]
    [DataTestMethod]
    public void JAssembly(int address, int expected) {
        var instruction = new J {
            Immediate = (address&0x3FFFFFF)>>2
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("J")]
    [DataRow("j 0x0040001c", 0x0040001C)]
    [DataRow("j 0x00400018", 0x00400018)]
    [DataTestMethod]
    public void JPopulate(string line, int target) {
        var instruction = new J();
        instruction.PopulateFromLine(line);
        Assert.AreEqual(target, instruction.Immediate);
    }

    [TestCategory("Jal")]
    [TestMethod]
    public void JalRegex() {
        var instruction = new Jal();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("jal 0x00400000"));
        Assert.IsTrue(regex.IsMatch("jal 0x00400004"));
        Assert.IsFalse(regex.IsMatch("j 0x0040000"));
    }

    [TestCategory("Jal")]
    [DataRow(0x0040001C, 0x0C100007)]
    [DataRow(0x00400018, 0x0C100006)]
    [DataTestMethod]
    public void JalAssembly(int address, int expected) {
        var instruction = new Jal {
            Immediate = (address&0x3FFFFFF)>>2
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Jal")]
    [DataRow("jal 0x0040001c", 0x0040001C)]
    [DataRow("jal 0x00400018", 0x00400018)]
    [DataTestMethod]
    public void JalPopulate(string line, int target) {
        var instruction = new Jal();
        instruction.PopulateFromLine(line);
        Assert.AreEqual(target, instruction.Immediate);
    }
}
