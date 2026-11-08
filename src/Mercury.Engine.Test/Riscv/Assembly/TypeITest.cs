using Mercury.Engine.Common;
using Mercury.Engine.Memory;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.Test.Riscv.Assembly;

/*
 * Type I format:
 *
 * | 31     20 | 19 15 | 14  12 | 11 7 | 6    0 | 
 * | imm[11:0] |  rs1  | funct3 |  rd  | opcode |
 */

// binary representations got from https://riscv-simulator-five.vercel.app/

[TestClass]
public class TypeITest {

    public const Endianness Little = Endianness.LittleEndian; 
    
    [TestMethod]
    [DataRow(0x00cf0267u,4,30,12)]
    [DataRow(0x00000067u,0,0,0)]
    [DataRow(0x7fff8fe7u,31,31,2047)]
    [DataRow(0xf8690767u,14,18,-122)]
    public void TestJalr(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Jalr instance = Assert.IsInstanceOfType<Jalr>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf0203u,4,30,12)]
    [DataRow(0x00000003u,0,0,0)]
    [DataRow(0x7fff8f83u,31,31,2047)]
    [DataRow(0xf8690703u,14,18,-122)]
    public void TestLb(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Lb instance = Assert.IsInstanceOfType<Lb>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf1203u,4,30,12)]
    [DataRow(0x00001003u,0,0,0)]
    [DataRow(0x7fff9f83u,31,31,2047)]
    [DataRow(0xf8691703u,14,18,-122)]
    public void TestLh(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Lh instance = Assert.IsInstanceOfType<Lh>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf2203u,4,30,12)]
    [DataRow(0x00002003u,0,0,0)]
    [DataRow(0x7fffaf83u,31,31,2047)]
    [DataRow(0xf8692703u,14,18,-122)]
    public void TestLw(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Lw instance = Assert.IsInstanceOfType<Lw>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf4203u,4,30,12)]
    [DataRow(0x00004003u,0,0,0)]
    [DataRow(0x7fffcf83u,31,31,2047)]
    [DataRow(0xf8694703u,14,18,-122)]
    public void TestLbu(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Lbu instance = Assert.IsInstanceOfType<Lbu>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf5203u,4,30,12)]
    [DataRow(0x00005003u,0,0,0)]
    [DataRow(0x7fffdf83u,31,31,2047)]
    [DataRow(0xf8695703u,14,18,-122)]
    public void TestLhu(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Lhu instance = Assert.IsInstanceOfType<Lhu>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf0213u,4,30,12)]
    [DataRow(0x00000013u,0,0,0)]
    [DataRow(0x7fff8f93u,31,31,2047)]
    [DataRow(0xf8690713u,14,18,-122)]
    public void TestAddi(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Addi instance = Assert.IsInstanceOfType<Addi>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf2213u,4,30,12)]
    [DataRow(0x00002013u,0,0,0)]
    [DataRow(0x7fffaf93u,31,31,2047)]
    [DataRow(0xf8692713u,14,18,-122)]
    public void TestSlti(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Slti instance = Assert.IsInstanceOfType<Slti>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf3213u,4,30,12)]
    [DataRow(0x00003013u,0,0,0)]
    [DataRow(0x7fffbf93u,31,31,2047)]
    [DataRow(0xf8693713u,14,18,-122)]
    public void TestSltiu(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Sltiu instance = Assert.IsInstanceOfType<Sltiu>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf4213u,4,30,12)]
    [DataRow(0x00004013u,0,0,0)]
    [DataRow(0x7fffcf93u,31,31,2047)]
    [DataRow(0xf8694713u,14,18,-122)]
    public void TestXori(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Xori instance = Assert.IsInstanceOfType<Xori>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf6213u,4,30,12)]
    [DataRow(0x00006013u,0,0,0)]
    [DataRow(0x7fffef93u,31,31,2047)]
    [DataRow(0xf8696713u,14,18,-122)]
    public void TestOri(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Ori instance = Assert.IsInstanceOfType<Ori>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf7213u,4,30,12)]
    [DataRow(0x00007013u,0,0,0)]
    [DataRow(0x7fffff93u,31,31,2047)]
    [DataRow(0xf8697713u,14,18,-122)]
    public void TestAndi(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Andi instance = Assert.IsInstanceOfType<Andi>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    /*
    [DataRow(u,4,30,12)]
    [DataRow(u,0,0,0)]
    [DataRow(u,31,31,2047)]
    [DataRow(u,14,18,-122)]
    public void Test(uint binary, int rd, int rs1, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        aaaaaa instance = Assert.IsInstanceOfType<aaaaa>(instruction);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Imm);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    */
}