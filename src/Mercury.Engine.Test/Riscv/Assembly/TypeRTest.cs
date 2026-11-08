using Mercury.Engine.Common;
using Mercury.Engine.Memory;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.Test.Riscv.Assembly;

/*
 * Type R format:
 *
 * | 31  25 | 24 20 | 19 15 | 14  12 | 11 7 | 6    0 |
 * | funct7 |  rs2  |  rs1  | funct3 |  rd  | opcode |
 */

// binarios teste calculados com https://riscv-simulator-five.vercel.app/
// lista de instrucoes puxada de 
// - https://www.cs.unh.edu/~pjh/courses/cs520/15spr/riscv-rv32i-instructions.pdf
// - https://msyksphinz-self.github.io/riscv-isadoc/#_rv32i_rv64i_instructions

[TestClass]
public class TypeRTest {

    public const Endianness Little = Endianness.LittleEndian;
    
    [TestMethod]
    [DataRow(0x003f1213u, 4, 30, 3)]
    [DataRow(0x00001013u, 0,0,0)]
    public void TestSlli(uint binary, int rd, int rs, int shamt) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Slli>(instruction);
        Slli instance = (Slli)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs, instance.Rs1);
        Assert.AreEqual(shamt, instance.Shamt);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x003f5213u, 4, 30, 3)]
    [DataRow(0x00005013u, 0,0,0)]
    public void TestSrli(uint binary, int rd, int rs, int shamt) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Srli>(instruction);
        Srli instance = (Srli)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs, instance.Rs1);
        Assert.AreEqual(shamt, instance.Shamt);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x403f5213u, 4, 30, 3)]
    [DataRow(0x40005013u, 0,0,0)]
    public void TestSrai(uint binary, int rd, int rs, int shamt) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Srai>(instruction);
        Srai instance = (Srai)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs, instance.Rs1);
        Assert.AreEqual(shamt, instance.Shamt);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf0233u, 4, 30, 12)]
    [DataRow(0x00000033u, 0,0,0)]
    [DataRow(0x01ff8fb3u, 31,31,31)]
    public void TestAdd(uint binary, int rd, int rs1, int rs2) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Add>(instruction);
        Add instance = (Add)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x40cf0233u, 4, 30, 12)]
    [DataRow(0x40000033u, 0,0,0)]
    [DataRow(0x41ff8fb3u, 31,31,31)]
    public void TestSub(uint binary, int rd, int rs1, int rs2) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Sub>(instruction);
        Sub instance = (Sub)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf1233u, 4, 30, 12)]
    [DataRow(0x00001033u, 0,0,0)]
    [DataRow(0x01ff9fb3u, 31,31,31)]
    public void TestSll(uint binary, int rd, int rs1, int rs2) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Sll>(instruction);
        Sll instance = (Sll)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf2233u, 4, 30, 12)]
    [DataRow(0x00002033u, 0,0,0)]
    [DataRow(0x01ffafb3u, 31,31,31)]
    public void TestSlt(uint binary, int rd, int rs1, int rs2) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Slt>(instruction);
        Slt instance = (Slt)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf3233u, 4, 30, 12)]
    [DataRow(0x00003033u, 0,0,0)]
    [DataRow(0x01ffbfb3u, 31,31,31)]
    public void TestSltu(uint binary, int rd, int rs1, int rs2) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Sltu>(instruction);
        Sltu instance = (Sltu)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf4233u, 4, 30, 12)]
    [DataRow(0x00004033u, 0,0,0)]
    [DataRow(0x01ffcfb3u, 31,31,31)]
    public void TestXor(uint binary, int rd, int rs1, int rs2) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Xor>(instruction);
        Xor instance = (Xor)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf5233u, 4, 30, 12)]
    [DataRow(0x00005033u, 0,0,0)]
    [DataRow(0x01ffdfb3u, 31,31,31)]
    public void TestSrl(uint binary, int rd, int rs1, int rs2) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Srl>(instruction);
        Srl instance = (Srl)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x40cf5233u, 4, 30, 12)]
    [DataRow(0x40005033u, 0,0,0)]
    [DataRow(0x41ffdfb3u, 31,31,31)]
    public void TestSra(uint binary, int rd, int rs1, int rs2) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Sra>(instruction);
        Sra instance = (Sra)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf6233u, 4, 30, 12)]
    [DataRow(0x00006033u, 0,0,0)]
    [DataRow(0x01ffefb3u, 31,31,31)]
    public void TestOr(uint binary, int rd, int rs1, int rs2) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<Or>(instruction);
        Or instance = (Or)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x00cf7233u, 4, 30, 12)]
    [DataRow(0x00007033u, 0,0,0)]
    [DataRow(0x01ffffb3u, 31,31,31)]
    public void TestAnd(uint binary, int rd, int rs1, int rs2) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Assert.IsInstanceOfType<And>(instruction);
        And instance = (And)instruction;
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(rs1, instance.Rs1);
        Assert.AreEqual(rs2, instance.Rs2);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
}