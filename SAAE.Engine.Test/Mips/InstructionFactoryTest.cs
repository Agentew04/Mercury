using SAAE.Engine.Mips.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Test.Mips;

[TestClass]
public class InstructionFactoryTest {

    [TestMethod]
    public void TestDisassembly() {
        int i1 = 0x20092af8;
        int i2 = 0x200a0899;
        int i3 = 0x012a0018;

        var factory = new InstructionFactory();
        var instruction1 = factory.Disassemble(i1);
        var instruction2 = factory.Disassemble(i2);
        var instruction3 = factory.Disassemble(i3);

        Assert.AreEqual(typeof(Addi), instruction1.GetType());
        Assert.AreEqual(11000, ((Addi)instruction1).Immediate);
        Assert.AreEqual(typeof(Addi), instruction2.GetType());
        Assert.AreEqual(2201, ((Addi)instruction2).Immediate);
        Assert.AreEqual(typeof(Mult), instruction3.GetType());
    }
}
