using System;
using System.Text.RegularExpressions;
using Mercury.Engine.Common;
using Mercury.Engine.Mips.Instructions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mercury.Engine.Test.Mips;

[TestClass]
public class TypeRTest {

    [TestCategory("Add")]
    [DataRow(8, 9, 10, 0x012A4020U)]
    [DataRow(0, 9, 31, 0x013F0020U)]
    [DataRow(23, 29, 26, 0x03BAB820U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void AddAssembly(int rd, int rs, int rt, uint result) {
        var instruction = new Add {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }
    
    [TestCategory("Add")]
    [DataRow(0x012A4020U)]
    [DataRow(0x013F0020U)]
    [DataRow(0x03BAB820U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void AddDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Add>(disassembled);
    }
    
    [TestCategory("Addu")]
    [DataRow(8, 9, 10, 0x012A4021U)]
    [DataRow(0, 9, 31, 0x013F0021U)]
    [DataRow(23, 29, 26, 0x03BAB821U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void AdduAssembly(int rd, int rs, int rt, uint result) {
        var instruction = new Addu {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }
    
    [TestCategory("Addu")]
    [DataRow(0x012A4021U)]
    [DataRow(0x013F0021U)]
    [DataRow(0x03BAB821U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void AdduDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Addu>(disassembled);
    }
    
    [TestCategory("Slt")]
    [DataRow(8, 9, 10, 0x012A402AU)]
    [DataRow(0, 9, 31, 0x013F002AU)]
    [DataRow(23, 29, 26, 0x03BAB82AU)]
    [TestMethod(DisplayName = "Test assembling")]
    public void SltAssembly(int rd, int rs, int rt, uint result) {
        var instruction = new Slt {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }
    
    [TestCategory("Slt")]
    [DataRow(0x012A402AU)]
    [DataRow(0x013F002AU)]
    [DataRow(0x03BAB82AU)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SltDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Slt>(disassembled);
    }

    [TestCategory("Sltu")]
    [DataRow(8, 9, 10, 0x012A402BU)]
    [DataRow(0, 9, 31, 0x013F002BU)]
    [DataRow(23, 29, 26, 0x03BAB82BU)]
    [TestMethod(DisplayName = "Test assembling")]
    public void SltuAssembly(int rd, int rs, int rt, uint result) {
        var instruction = new Sltu {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sltu")]
    [DataRow(0x012A402BU)]
    [DataRow(0x013F002BU)]
    [DataRow(0x03BAB82BU)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SltuDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sltu>(disassembled);
    }
    
    [TestCategory("Sub")]
    [DataRow(8, 9, 10, 0x012A4022U)]
    [DataRow(0, 9, 31, 0x013F0022U)]
    [DataRow(23, 29, 26, 0x03BAB822U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void SubAssembly(int rd, int rs, int rt, uint result) {
        var instruction = new Sub {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sub")]
    [DataRow(0x012A4022U)]
    [DataRow(0x013F0022U)]
    [DataRow(0x03BAB822U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SubDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sub>(disassembled);
    }

    [TestCategory("Subu")]
    [DataRow(8, 9, 10, 0x012A4023U)]
    [DataRow(0, 9, 31, 0x013F0023U)]
    [DataRow(23, 29, 26, 0x03BAB823U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void SubuAssembly(int rd, int rs, int rt, uint result) {
        var instruction = new Subu {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Subu")]
    [DataRow(0x012A4023U)]
    [DataRow(0x013F0023U)]
    [DataRow(0x03BAB823U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SubuDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Subu>(disassembled);
    }
    
    [TestCategory("And")]
    [DataRow(8, 9, 10, 0x012A4024U)]
    [DataRow(0, 9, 31, 0x013F0024U)]
    [DataRow(23, 29, 26, 0x03BAB824U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void AndAssembly(int rd, int rs, int rt, uint result) {
        var instruction = new And {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("And")]
    [DataRow((uint)0x012A4024)]
    [DataRow((uint)0x013F0024)]
    [DataRow((uint)0x03BAB824)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void AndDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<And>(disassembled);
    }

    [TestCategory("Nor")]
    [DataRow(8, 9, 10, 0x012A4027U)]
    [DataRow(0, 9, 31, 0x013F0027U)]
    [DataRow(23, 29, 26, 0x03BAB827U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void NorAssembly(int rd, int rs, int rt, uint result) {
        var instruction = new Nor {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Nor")]
    [DataRow((uint)0x012A4027)]
    [DataRow((uint)0x013F0027)]
    [DataRow((uint)0x03BAB827)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void NorDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Nor>(disassembled);
    }
    
    [TestCategory("Or")]
    [DataRow(8, 9, 10, 0x012A4025U)]
    [DataRow(0, 9, 31, 0x013F0025U)]
    [DataRow(23, 29, 26, 0x03BAB825U)]
    [DataRow(23, 26, 1, 0x0341B825U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void OrAssembly(int rd, int rs, int rt, uint result) {
        var instruction = new Or {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Or")]
    [DataRow(0x012A4025U)]
    [DataRow(0x013F0025U)]
    [DataRow(0x03BAB825U)]
    [DataRow(0x0341B825U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void OrDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Or>(disassembled);
    }

    [TestCategory("Xor")]
    [DataRow(8, 9, 10, 0x012A4026U)]
    [DataRow(0, 9, 31, 0x013F0026U)]
    [DataRow(23, 29, 26, 0x03BAB826U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void XorAssembly(int rd, int rs, int rt, uint result) {
        var instruction = new Xor {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Xor")]
    [DataRow(0x012A4026U)]
    [DataRow(0x013F0026U)]
    [DataRow(0x03BAB826U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void XorDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Xor>(disassembled);
    }
    
    [TestCategory("Sll")]
    [DataRow(8, 9, 1, 0x00094040U)]
    [DataRow(0, 9, 10, 0x00090280U)]
    [DataRow(23, 29, 5, 0X001DB940U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void SllAssembly(int rd, int rt, int shamt, uint result) {
        var instruction = new Sll {
            Rd = (byte)rd,
            Rt = (byte)rt,
            ShiftAmount = (byte)shamt
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sll")]
    [DataRow(0x00094040U)]
    [DataRow(0x00090280U)]
    [DataRow(0x001DB940U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SllDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sll>(disassembled);
    }

    [TestCategory("Sllv")]
    [DataRow(8, 9, 10, 0x01494004U)]
    [DataRow(0, 9, 31, 0x03E90004U)]
    [DataRow(23, 29, 26, 0x035DB804U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void SllvAssembly(int rd, int rt, int rs, uint result) {
        var instruction = new Sllv {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt(), $"EXP:{Convert.ToHexString(BitConverter.GetBytes(result))}\nREA:{Convert.ToHexString(BitConverter.GetBytes(instruction.ConvertToInt()))}");
    }

    [TestCategory("Sllv")]
    [DataRow(0x01494004U)]
    [DataRow(0x03E90004U)]
    [DataRow(0x035DB804U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SllvDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sllv>(disassembled);
    }

    [TestCategory("Sra")]
    [DataRow(8, 9, 1, 0x00094043U)]
    [DataRow(0, 9, 10, 0x00090283U)]
    [DataRow(23, 29, 5, 0x001DB943U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void SraAssembly(int rd, int rt, int shamt, uint result) {
        var instruction = new Sra {
            Rd = (byte)rd,
            Rt = (byte)rt,
            ShiftAmount = (byte)shamt
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sra")]
    [DataRow(0x00094043U)]
    [DataRow(0x00090283U)]
    [DataRow(0x001DB943U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SraDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sra>(disassembled);
    }
    
    [TestCategory("Srav")]
    [DataRow(8, 9, 10, 0x01494007U)]
    [DataRow(0, 9, 31, 0x03E90007U)]
    [DataRow(23, 29, 26, 0x035DB807U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void SravAssembly(int rd, int rt, int rs, uint result) {
        var instruction = new Srav {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Srav")]
    [DataRow(0x01494007U)]
    [DataRow(0x03E90007U)]
    [DataRow(0x035DB807U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SravDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Srav>(disassembled);
    }

    [TestCategory("Srl")]
    [DataRow(8, 9, 1, 0x00094042U)]
    [DataRow(0, 9, 10, 0x00090282U)]
    [DataRow(23, 29, 5, 0x001DB942U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void SrlAssembly(int rd, int rt, int shamt, uint result) {
        var instruction = new Srl {
            Rd = (byte)rd,
            Rt = (byte)rt,
            ShiftAmount = (byte)shamt
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Srl")]
    [DataRow((uint)0x00094042)]
    [DataRow((uint)0x00090282)]
    [DataRow((uint)0x001DB942)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SrlDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Srl>(disassembled);
    }
    
    [TestCategory("Srlv")]
    [DataRow(8, 9, 10, 0x01494006U)]
    [DataRow(0, 9, 31, 0x03E90006U)]
    [DataRow(23, 29, 26, 0x035DB806U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void SrlvAssembly(int rd, int rt, int rs, uint result) {
        var instruction = new Srlv {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Srlv")]
    [DataRow(0x01494006U)]
    [DataRow(0x03E90006U)]
    [DataRow(0x035DB806U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SrlvDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Srlv>(disassembled);
    }

    [TestCategory("Div")]
    [DataRow(8, 9, 0x0109001AU)]
    [DataRow(0, 10, 0x000A001AU)]
    [DataRow(23, 26, 0x02FA001AU)]
    [TestMethod(DisplayName = "Test assembling")]
    public void DivAssembly(int rs, int rt, uint result) {
        var instruction = new Div {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Div")]
    [DataRow((uint)0x0109001A)]
    [DataRow((uint)0x000A001A)]
    [DataRow((uint)0x02FA001A)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void DivDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Div>(disassembled);
    }

    [TestCategory("Divu")]
    [DataRow(8, 9, 0x0109001BU)]
    [DataRow(0, 10, 0x000A001BU)]
    [DataRow(23, 26, 0x02FA001BU)]
    [TestMethod(DisplayName = "Test assembling")]
    public void DivuAssembly(int rs, int rt, uint result) {
        var instruction = new Divu {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Divu")]
    [DataRow(0x0109001BU)]
    [DataRow(0x000A001BU)]
    [DataRow(0x02FA001BU)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void DivuDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Divu>(disassembled);
    }

    [TestCategory("Mult")]
    [DataRow(8, 9, 0x01090018U)]
    [DataRow(0, 10, 0x000A0018U)]
    [DataRow(23, 26, 0x02FA0018U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void MultAssembly(int rs, int rt, uint result) {
        var instruction = new Mult {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mult")]
    [DataRow(0x01090018U)]
    [DataRow(0x000A0018U)]
    [DataRow(0x02FA0018U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void MultDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mult>(disassembled);
    }
    
    [TestCategory("Multu")]
    [DataRow(8, 9, 0x01090019U)]
    [DataRow(0, 10, 0x000A0019U)]
    [DataRow(23, 26, 0x02FA0019U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void MultuAssembly(int rs, int rt, uint result) {
        var instruction = new Multu {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Multu")]
    [DataRow(0x01090019U)]
    [DataRow(0x000A0019U)]
    [DataRow(0x02FA0019U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void MultuDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Multu>(disassembled);
    }
    
    [TestCategory("Mfhi")]
    [DataRow(31, 0x0000F810U)]
    [DataRow(0, 0x00000010U)]
    [DataRow(20, 0x0000A010U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void MfhiAssembly(int rd, uint result) {
        var instruction = new Mfhi {
            Rd = (byte)rd,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mfhi")]
    [DataRow(0x0000F810U)]
    [DataRow(0x00000010U)]
    [DataRow(0x0000A010U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void MfhiDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mfhi>(disassembled);
    }

    [TestCategory("Mflo")]
    [DataRow(31, 0x0000F812U)]
    [DataRow(0, 0x00000012U)]
    [DataRow(20, 0x0000A012U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void MfloAssembly(int rd, uint result) {
        var instruction = new Mflo {
            Rd = (byte)rd,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mflo")]
    [DataRow(0x0000F812U)]
    [DataRow(0x00000012U)]
    [DataRow(0x0000A012U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void MfloDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mflo>(disassembled);
    }
    
    [TestCategory("Mthi")]
    [DataRow(31, 0x03E00011U)]
    [DataRow(0, 0x00000011U)]
    [DataRow(20, 0x02800011U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void MthiAssembly(int rs, uint result) {
        var instruction = new Mthi {
            Rs = (byte)rs,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mthi")]
    [DataRow(0x03E00011U)]
    [DataRow(0x00000011U)]
    [DataRow(0x02800011U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void MthiDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mthi>(disassembled);
    }

    [TestCategory("Mtlo")]
    [DataRow(31, 0x03E00013U)]
    [DataRow(0, 0x00000013U)]
    [DataRow(20, 0x02800013U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void MtloAssembly(int rs, uint result) {
        var instruction = new Mtlo {
            Rs = (byte)rs,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mtlo")]
    [DataRow(0x03E00013U)]
    [DataRow(0x00000013U)]
    [DataRow(0x02800013U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void MtloDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mtlo>(disassembled);
    }
    
    [TestCategory("Clo")]
    [DataRow(31, 27, 0x7360F821U)]
    [DataRow(18, 12, 0x71809021U)]
    [DataRow(29, 0, 0x7000E821U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void CloAssembly(int rd, int rs, uint result) {
        var instruction = new Clo {
            Rd = (byte)rd,
            Rs = (byte)rs,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Clo")]
    [DataRow(0x7360F821U)]
    [DataRow(0x71809021U)]
    [DataRow(0x7000E821U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void CloDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Clo>(disassembled);
    }
    
    [TestCategory("Clz")]
    [DataRow(31, 27, 0x7360F820U)]
    [DataRow(18, 12, 0x71809020U)]
    [DataRow(29, 0, 0x7000E820U)]
    [TestMethod(DisplayName = "Test assembling")]
    public void ClzAssembly(int rd, int rs, uint result) {
        var instruction = new Clz {
            Rd = (byte)rd,
            Rs = (byte)rs,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Clz")]
    [DataRow(0x7360F820U)]
    [DataRow(0x71809020U)]
    [DataRow(0x7000E820U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void ClzDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Clz>(disassembled);
    }

    [TestCategory("Jalr")]
    [DataRow(28, 0x0380F809U)]
    [DataRow(19, 0x0260F809U)]
    [TestMethod]
    public void JalrAssemblySingle(int rs, uint result) {
        var instruction = new Jalr {
            Rs = (byte)rs,
            Rd = 31 // $ra
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Jalr")]
    [DataRow(28, 10, 0x0140E009U)]
    [DataRow(19, 4, 0x00809809U)]
    [TestMethod]
    public void JalrAssemblyDouble(int rd, int rs, uint result) {
        var instruction = new Jalr {
            Rd = (byte)rd,
            Rs = (byte)rs,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }
    
    [TestCategory("Syscall")]
    [DataRow(0,0x0000000CU)]
    [DataRow(2,0b000000_00000000000000000010_001100U)]
    [TestMethod(DisplayName = "Test Syscall Assembly")]
    public void SyscallAssembly(int code, uint result) {
        var instruction = new Syscall {
            Code = code
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Jalr")]
    [DataRow(0x0380F809U)]
    [DataRow(0x0260F809U)]
    [DataRow((uint)0x0140E009)]
    [DataRow((uint)0x00809809)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void JalrDisassemblySingle(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Jalr>(disassembled);
    }
    
    [TestCategory("Syscall")]
    [DataRow(0x0000000CU)]
    [DataRow(0b000000_00000000000000000010_001100U)]
    [TestMethod(DisplayName = "Test disassembling")]
    public void SyscallDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Syscall>(disassembled);
    }

    
}