using Mercury.Engine.Common;
using Mercury.Engine.Memory;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.Test.Riscv.Assembly;

/*
 * Type SB format:
 *
 * | 31   31 | 30     25 | 24 20 | 19 15 | 14  12 | 11 6     | 7     7 | 6    0 |
 * | imm[12] | imm[10:5] |  rs2  |  rs1  | funct3 | imm[4:1] | imm[11] | opcode |
 */

[TestClass]
public class TypeSBTest {

    public const Endianness Little = Endianness.LittleEndian;
    
    [TestMethod]
    [DataRow(0x01e20663u, 4, 30, 12)]
    [DataRow(0x00000063u, 0, 0, 0)]
    [DataRow(0x01ff80e3u, 31, 31, 2048)]
    [DataRow(0xf92703e3u, 14, 18, -122)]
    public void TestBeq(uint binary, int rs1, int rs2, int imm) { // rs2 contains data (first on asm), rs1 base addr
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Beq instance = Assert.IsInstanceOfType<Beq>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x01e21663u, 4, 30, 12)]
    [DataRow(0x00001063u, 0, 0, 0)]
    [DataRow(0x01ff90e3u, 31, 31, 2048)]
    [DataRow(0xf92713e3u, 14, 18, -122)]
    public void TestBne(uint binary, int rs1, int rs2, int imm) { // rs2 contains data (first on asm), rs1 base addr
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Bne instance = Assert.IsInstanceOfType<Bne>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x01e24663u, 4, 30, 12)]
    [DataRow(0x00004063u, 0, 0, 0)]
    [DataRow(0x01ffc0e3u, 31, 31, 2048)]
    [DataRow(0xf92743e3u, 14, 18, -122)]
    public void TestBlt(uint binary, int rs1, int rs2, int imm) { // rs2 contains data (first on asm), rs1 base addr
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Blt instance = Assert.IsInstanceOfType<Blt>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x01e25663u, 4, 30, 12)]
    [DataRow(0x00005063u, 0, 0, 0)]
    [DataRow(0x01ffd0e3u, 31, 31, 2048)]
    [DataRow(0xf92753e3u, 14, 18, -122)]
    public void TestBge(uint binary, int rs1, int rs2, int imm) { // rs2 contains data (first on asm), rs1 base addr
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Bge instance = Assert.IsInstanceOfType<Bge>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x01e26663u, 4, 30, 12)]
    [DataRow(0x00006063u, 0, 0, 0)]
    [DataRow(0x01ffe0e3u, 31, 31, 2048)]
    [DataRow(0xf92763e3u, 14, 18, -122)]
    public void TestBltu(uint binary, int rs1, int rs2, int imm) { // rs2 contains data (first on asm), rs1 base addr
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Bltu instance = Assert.IsInstanceOfType<Bltu>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x01e27663u, 4, 30, 12)]
    [DataRow(0x00007063u, 0, 0, 0)]
    [DataRow(0x01fff0e3u, 31, 31, 2048)]
    [DataRow(0xf92773e3u, 14, 18, -122)]
    public void TestBgeu(uint binary, int rs1, int rs2, int imm) { // rs2 contains data (first on asm), rs1 base addr
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Bgeu instance = Assert.IsInstanceOfType<Bgeu>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    
}