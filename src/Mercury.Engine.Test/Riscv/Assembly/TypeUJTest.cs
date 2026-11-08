using Mercury.Engine.Common;
using Mercury.Engine.Memory;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.Test.Riscv.Assembly;

/*
 * Type UJ format:
 *
 * | 31   31 | 30     21 | 20   20 | 19      12 | 11 7 | 6    0 |
 * | imm[20] | imm[10:1] | imm[11] | imm[19:12] |  rd  | opcode |
 */

[TestClass]
public class TypeUJTest {

    public const Endianness Little = Endianness.LittleEndian;

    [TestMethod]
    [DataRow(0x00c0026fu, 4, 12)]
    [DataRow(0x0000006fu, 0, 0)]
    [DataRow(0x00100fefu, 31, 2048)]
    [DataRow(0xf87ff76fu, 14, -122)]
    public void TestJal(uint binary, int rd, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Jal instance = Assert.IsInstanceOfType<Jal>(instruction);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
}