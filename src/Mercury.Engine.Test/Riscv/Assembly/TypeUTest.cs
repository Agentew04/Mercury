using Mercury.Engine.Common;
using Mercury.Engine.Memory;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.Test.Riscv.Assembly;

/*
 * Type U format:
 *
 * | 31     12  | 11 7 | 6    0 |
 * | imm[31:12] |  rd  | opcode |
 */

[TestClass]
public class TypeUTest {

    public const Endianness Little = Endianness.LittleEndian;
    
    [TestMethod]
    [DataRow(0x0000c237u, 4, 12)]
    [DataRow(0x00000037u, 0, 0)]
    [DataRow(0x00800fb7u, 31, 2048)]
    [DataRow(0xfff86737u, 14, -122)]
    public void TestLui(uint binary, int rd, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Lui instance = Assert.IsInstanceOfType<Lui>(instruction);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Immediate);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
    
    [TestMethod]
    [DataRow(0x0000c217u, 4, 12)]
    [DataRow(0x00000017u, 0, 0)]
    [DataRow(0x00800f97u, 31, 2048)]
    [DataRow(0xfff86717u, 14, -122)]
    public void TestAuipc(uint binary, int rd, int imm) {
        IInstruction? instruction = Disassembler.Disassemble(new Int32ByteSource(binary, Little), new InstructionPool());
        
        Assert.IsNotNull(instruction);
        Auipc instance = Assert.IsInstanceOfType<Auipc>(instruction);
        Assert.AreEqual(rd, instance.Rd);
        Assert.AreEqual(imm, instance.Immediate);
        Assert.AreEqual(binary, instance.ConvertToInt(Little));
    }
}