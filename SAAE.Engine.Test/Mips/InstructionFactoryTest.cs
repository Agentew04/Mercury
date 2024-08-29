using SAAE.Engine.Mips.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Test.Mips;

[TestClass]
public class InstructionFactoryTest {

    private InstructionFactory factory = new();

    [TestMethod]
    public void TestDisassemblyTypeI() {
        int i1 = 0x20092af8;
        int i2 = 0x200a0899;
        int i3 = 0x1200fffe;

        var instruction1 = factory.Disassemble(i1) as Addi;
        var instruction2 = factory.Disassemble(i2) as Addi;
        var instruction3 = factory.Disassemble(i3) as Beq;

        Assert.IsNotNull(instruction1);
        Assert.AreEqual(9, instruction1.Rt);
        Assert.AreEqual(0, instruction1.Rs);
        Assert.AreEqual(11000, instruction1.Immediate);
        Assert.AreEqual(i1, instruction1.ConvertToInt());

        Assert.IsNotNull(instruction2);
        Assert.AreEqual(10, instruction2.Rt);
        Assert.AreEqual(0, instruction2.Rs);
        Assert.AreEqual(2201, instruction2.Immediate);
        Assert.AreEqual(i2, instruction2.ConvertToInt());

        Assert.IsNotNull(instruction3);
        Assert.AreEqual(16, instruction3.Rs);
        Assert.AreEqual(0, instruction3.Rt);
        Assert.AreEqual(-2, instruction3.Immediate);
        Assert.AreEqual(i3, instruction3.ConvertToInt());
    }

    [TestMethod]
    public void TestDisassemblyTypeR() {
        int i1 = 0x00e38022;
        int i2 = 0x012a001a;

        var instruction1 = factory.Disassemble(i1) as Sub;
        var instruction2 = factory.Disassemble(i2) as Div;

        Assert.IsNotNull(instruction1);
        Assert.AreEqual(16, instruction1.Rd);
        Assert.AreEqual(7, instruction1.Rs);
        Assert.AreEqual(3, instruction1.Rt);
        Assert.AreEqual(i1, instruction1.ConvertToInt());

        Assert.IsNotNull(instruction2);
        Assert.AreEqual(9, instruction2.Rs);
        Assert.AreEqual(10, instruction2.Rt);
        Assert.AreEqual(0, instruction2.Rd);
        Assert.AreEqual(i2, instruction2.ConvertToInt());
    }

    [TestMethod]
    public void TestDisassemblyTypeJ() {
        int i1 = 0x08100002;

        var instruction1 = factory.Disassemble(i1) as J;

        Assert.IsNotNull(instruction1);
        Assert.AreEqual((0x0040_0008 & 0x3FFFFFF)>>2, instruction1.Immediate);
        Assert.AreEqual(i1, instruction1.ConvertToInt());
    }
}
