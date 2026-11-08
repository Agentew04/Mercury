using Mercury.Engine.Common;
using Mercury.Engine.Memory;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.Test.Riscv.Assembly;

/*
 * Type S format:
 *
 * | 31     25 | 24 20 | 19 15 | 14  12 | 11      7 | 6    0 |
 * | imm[11:5] |  rs2  |  rs1  | funct3 |  imm[4:0] | opcode |
 */

[TestClass]
public class TypeSTest {

    public const Endianness Little = Endianness.LittleEndian;
    
    [TestMethod]
    [DataRow(0x004f0623u, 4, 30, 12)]
    [DataRow(0x00000023u, 0, 0, 0)]
    [DataRow(0x7fff8fa3u, 31, 31, 2047)]
    [DataRow(0xf8e90323u, 14, 18, -122)]
    public void TestSb(uint binary, int rs2, int rs1, int imm) { // rs2 contains data (first on asm), rs1 base addr
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Sb instance = Assert.IsInstanceOfType<Sb>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x004f1623u, 4, 30, 12)]
    [DataRow(0x00001023u, 0, 0, 0)]
    [DataRow(0x7fff9fa3u, 31, 31, 2047)]
    [DataRow(0xf8e91323u, 14, 18, -122)]
    public void TestSh(uint binary, int rs2, int rs1, int imm) { // rs2 contains data (first on asm), rs1 base addr
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Sh instance = Assert.IsInstanceOfType<Sh>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x004f2623u, 4, 30, 12)]
    [DataRow(0x00002023u, 0, 0, 0)]
    [DataRow(0x7fffafa3u, 31, 31, 2047)]
    [DataRow(0xf8e92323u, 14, 18, -122)]
    public void TestSw(uint binary, int rs2, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Sw instance = Assert.IsInstanceOfType<Sw>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    /*
    [TestMethod]
    [DataRow(u, 4, 30, 12)]
    [DataRow(u, 0, 0, 0)]
    [DataRow(u, 31, 31, 2047)]
    [DataRow(u, 14, 18, -122)]
    public void Test(uint binary, int rs1, int rs2, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Sb instance = Assert.IsInstanceOfType<Sb>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
     */
}