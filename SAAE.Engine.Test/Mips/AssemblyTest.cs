using SAAE.Engine.Mips.Assembler;


namespace SAAE.Engine.Test.Mips;

[TestClass]
public class AssemblyTest {

    [TestMethod]
    public void TestAssembly1() {
        string code = """
            add $t1, $zero, $s0
            sub $t1, $0, $s1
            or $s7, $k0, $1
            xor $27, $k1, $ra
            srav $t1, $t2, $a3
            sll $fp, $v0, 5
            """;
        byte[] expected = [
            0x20, 0x48, 0x10, 0x00,
            0x22, 0x48, 0x11, 0x00,
            0x25, 0xb8, 0x41, 0x03,
            0x26, 0xd8, 0x7f, 0x03,
            0x07, 0x48, 0xea, 0x00,
            0x40, 0xf1, 0x02, 0x00
        ];
        byte[] actual = new MipsAssembler().Assemble(code);

        CollectionAssert.AreEqual(expected, actual);
    }
}
