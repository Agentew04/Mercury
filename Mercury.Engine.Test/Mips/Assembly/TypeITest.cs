using Mercury.Engine.Common;
using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Test.Mips;

[TestClass]
public class TypeITest {

    [TestCategory("Addi")]
    [DataRow(8, 0, 0xF, 0x2008000FU)]
    [DataRow(0, 10, 0x8F, 0x2140008FU)]
    [DataRow(23,29, 0xFFE4, 0x23B7FFE4U)]
    [TestMethod]
    public void AddiAssembly(int rt, int rs, int immediate, uint expected) {
        var instruction = new Addi {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }
    
    [TestCategory("Addi")]
    [DataRow(0x2008000FU)]
    [DataRow(0x2140008FU)]
    [DataRow(0x23B7FFE4U)]
    [TestMethod]
    public void AddiDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Addi>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Addiu")]
    [DataRow(8, 0, 0xF, 0x2408000FU)]
    [DataRow(0, 10, 0x8F, 0x2540008FU)]
    [DataRow(23,29, 0x7FE4, 0x27B77FE4U)]
    [TestMethod]
    public void AddiuAssembly(int rt, int rs, int immediate, uint expected) {
        Addiu instruction = new() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Addiu")]
    [DataRow(0x2408000FU)]
    [DataRow(0x2540008FU)]
    [DataRow(0x27B77FE4U)]
    [TestMethod]
    public void AddiuDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Addiu>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Slti")]
    [DataRow(8, 0, 0xF, 0x2808000FU)]
    [DataRow(0, 10, 0x8F, 0x2940008FU)]
    [DataRow(23,29, 0xFFE4, 0x2BB7FFE4U)]
    [TestMethod]
    public void SltiAssembly(int rt, int rs, int immediate, uint expected) {
        var instruction = new Slti() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Slti")]
    [DataRow(0x2808000FU)]
    [DataRow(0x2940008FU)]
    [DataRow(0x2BB7FFE4U)]
    [TestMethod]
    public void SltiDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Slti>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Sltiu")]
    [DataRow(8, 0, 0xF, 0x2C08000FU)]
    [DataRow(0, 10, 0x8F, 0x2D40008FU)]
    [DataRow(23,29, 0xFFE4, 0x2FB7FFE4U)]
    [TestMethod]
    public void SltiuAssembly(int rt, int rs, int immediate, uint expected) {
        var instruction = new Sltiu() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Sltiu")]
    [DataRow(0x2C08000FU)]
    [DataRow(0x2D40008FU)]
    [DataRow(0x2FB7FFE4U)]
    [TestMethod]
    public void SltiuDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sltiu>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Andi")]
    [DataRow(8, 0, 0xF, 0x3008000FU)]
    [DataRow(0, 10, 0x8F, 0x3140008FU)]
    [TestMethod]
    public void AndiAssembly(int rt, int rs, int immediate, uint expected) {
        var instruction = new Andi() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Andi")]
    [DataRow(0x3008000FU)]
    [DataRow(0x3140008FU)]
    [TestMethod]
    public void AndiDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Andi>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Ori")]
    [DataRow(8, 0, 0xF, 0x3408000FU)]
    [DataRow(0, 10, 0x8F, 0x3540008FU)]
    [TestMethod]
    public void OriAssembly(int rt, int rs, int immediate, uint expected) {
        var instruction = new Ori() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Ori")]
    [DataRow(0x3408000FU)]
    [DataRow(0x3540008FU)]
    [TestMethod]
    public void OriDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Ori>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }
    
    [TestCategory("Xori")]
    [DataRow(8, 0, 0xF, 0x3808000FU)]
    [DataRow(0, 10, 0x8F, 0x3940008FU)]
    [TestMethod]
    public void XoriAssembly(int rt, int rs, int immediate, uint expected) {
        var instruction = new Xori() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Xori")]
    [DataRow(0x3808000FU)]
    [DataRow(0x3940008FU)]
    [TestMethod]
    public void XoriDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Xori>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Lb")]
    [DataRow(1, 0xF, 11, 0x8161000FU)]
    [DataRow(27, 0xFFF1, 28, 0x839BFFF1U)]
    [DataRow(31, 0x1, 23, 0x82FF0001U)]
    [TestMethod]
    public void LbAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Lb() {
            Rt = (byte)rt,
            Base = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Lb")]
    [DataRow(0x8161000FU)]
    [DataRow(0x839BFFF1U)]
    [DataRow(0x82FF0001U)]
    [TestMethod]
    public void LbDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Lb>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }
    
    [TestCategory("Lbu")]
    [DataRow(1, 0xF, 11, 0x9161000FU)]
    [DataRow(27, 0xFFF1, 28, 0x939BFFF1U)]
    [DataRow(31, 0x1, 23, 0x92FF0001U)]
    [TestMethod]
    public void LbuAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Lbu {
            Rt = (byte)rt,
            Base = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Lbu")]
    [DataRow(0x9161000FU)]
    [DataRow(0x939BFFF1U)]
    [DataRow(0x92FF0001U)]
    [TestMethod]
    public void LbuDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Lbu>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }
    
    [TestCategory("Lh")]
    [DataRow(1, 0xF, 11, 0x8561000FU)]
    [DataRow(27, 0xFFF1, 28, 0x879BFFF1U)]
    [DataRow(31, 0x1, 23, 0x86FF0001U)]
    [TestMethod]
    public void LhAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Lh() {
            Rt = (byte)rt,
            Base = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Lh")]
    [DataRow(0x8561000FU)]
    [DataRow(0x879BFFF1U)]
    [DataRow(0x86FF0001U)]
    [TestMethod]
    public void LhDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Lh>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Lhu")]
    [DataRow(1, 0xF, 11, 0x9561000FU)]
    [DataRow(27, 0xFFF1, 28, 0x979BFFF1U)]
    [DataRow(31, 0x1, 23, 0x96FF0001U)]
    [TestMethod]
    public void LhuAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Lhu() {
            Rt = (byte)rt,
            Base = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Lhu")]
    [DataRow(0x9561000FU)]
    [DataRow(0x979BFFF1U)]
    [DataRow(0x96FF0001U)]
    [TestMethod]
    public void LhuDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Lhu>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }
    
    [TestCategory("Lui")]
    [DataRow(1, 0xF, 0x3C01000FU)]
    [DataRow(27, 0, 0x3C1B0000U)]
    [DataRow(31, 0xFA, 0x3C1F00FAU)]
    [TestMethod]
    public void LuiAssembly(int rt, int immediate, uint expected) {
        var instruction = new Lui {
            Rt = (byte)rt,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Lui")]
    [DataRow(0x3C01000FU)]
    [DataRow(0x3C1B0000U)]
    [DataRow(0x3C1F00FAU)]
    [TestMethod]
    public void LuiDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Lui>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Lw")]
    [DataRow(1, 0xF, 11, 0x8D61000FU)]
    [DataRow(27, 0x7FF1, 28, 0x8F9B7FF1U)]
    [DataRow(31, 0x1, 23, 0x8EFF0001U)]
    [TestMethod]
    public void LwAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Lw() {
            Rt = (byte)rt,
            Base = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Lw")]
    [DataRow(0x8D61000FU)]
    [DataRow(0x8F9B7FF1U)]
    [DataRow(0x8EFF0001U)]
    [TestMethod]
    public void LwDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Lw>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }
    
    [TestCategory("Sb")]
    [DataRow(1, 0xF, 11, 0xA161000FU)]
    [DataRow(27, 0x7FF1, 28, 0xA39B7FF1U)]
    [DataRow(31, 0x1, 23, 0xA2FF0001U)]
    [TestMethod]
    public void SbAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Sb() {
            Rt = (byte)rt,
            Base = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Sb")]
    [DataRow(0xA161000FU)]
    [DataRow(0xA39B7FF1U)]
    [DataRow(0xA2FF0001U)]
    [TestMethod]
    public void SbDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sb>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Sh")]
    [DataRow(1, 0xF, 11, 0xA561000FU)]
    [DataRow(27, 0x7FF1, 28, 0xA79B7FF1U)]
    [DataRow(31, 0x1, 23, 0xA6FF0001U)]
    [TestMethod]
    public void ShAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Sh() {
            Rt = (byte)rt,
            Base = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Sh")]
    [DataRow(0xA561000FU)]
    [DataRow(0xA79B7FF1U)]
    [DataRow(0xA6FF0001U)]
    [TestMethod]
    public void ShDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sh>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Sw")]
    [DataRow(1, 0xF, 11, 0xAD61000FU)]
    [DataRow(27, 0x7FF1, 28, 0xAF9B7FF1U)]
    [DataRow(31, 0x1, 23, 0xAEFF0001U)]
    [TestMethod]
    public void SwAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Sw() {
            Rt = (byte)rt,
            Base = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Sw")]
    [DataRow(0xAD61000FU)]
    [DataRow(0xAF9B7FF1U)]
    [DataRow(0xAEFF0001U)]
    [TestMethod]
    public void SwDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sw>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Beq")]
    [DataRow(7, 28, 0x03F03FFF, 0x10FC3FFFU)]
    [DataRow(22, 12, 0x18, 0x12CC0018U)]
    [DataRow(0, 3, 0x03F03FFD, 0x10033FFDU)]
    [DataRow(10, 8, 0x2, 0x11480002U)]
    [TestMethod]
    public void BeqAssembly(int rs, int rt, int immediate, uint expected) {
        var instruction = new Beq {
            Rs = (byte)rs,
            Rt = (byte)rt,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Bgez")]
    [DataRow(7, 0x03F03FFF, 0x04E13FFFU)]
    [DataRow(22, 0x18, 0x06C10018U)]
    [DataRow(0, 0x03F03FFD, 0x04013FFDU)]
    [TestMethod]
    public void BgezAssembly(int rs, int immediate, uint expected) {
        var instruction = new Bgez() {
            Rs = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Bgez")]
    [DataRow(0x04E13FFFU)]
    [DataRow(0x06C10018U)]
    [DataRow(0x04013FFDU)]
    [TestMethod]
    public void BgezDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Bgez>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Bgtz")]
    [DataRow(7, 0x03F03FFF, 0x1CE03FFFU)]
    [DataRow(22, 0x18, 0x1EC00018U)]
    [DataRow(0, 0x03F03FFD, 0x1C003FFDU)]
    [TestMethod]
    public void BgtzAssembly(int rs, int immediate, uint expected) {
        var instruction = new Bgtz {
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Bgtz")]
    [DataRow(0x1CE03FFFU)]
    [DataRow(0x1EC00018U)]
    [DataRow(0x1C003FFDU)]
    [TestMethod]
    public void BgtzDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Bgtz>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }
    
    [TestCategory("Blez")]
    [DataRow(7, 0x03F03FFF, 0x18E03FFFU)]
    [DataRow(22, 0x18, 0x1AC00018U)]
    [DataRow(0, 0x03F03FFD, 0x18003FFDU)]
    [TestMethod]
    public void BlezAssembly(int rs, int immediate, uint expected) {
        var instruction = new Blez {
            Rs = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Blez")]
    [DataRow(0x18E03FFFU)]
    [DataRow(0x1AC00018U)]
    [DataRow(0x18003FFDU)]
    [TestMethod]
    public void BlezDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Blez>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }
    
    [TestCategory("Bltz")]
    [DataRow(7, 0x03F03FFF, 0x04E03FFFU)]
    [DataRow(22, 0x18, 0x06C00018U)]
    [DataRow(0, 0x03F03FFD, 0x04003FFDU)]
    [TestMethod]
    public void BltzAssembly(int rs, int immediate, uint expected) {
        var instruction = new Bltz {
            Rs = (byte)rs,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Bltz")]
    [DataRow(0x04E03FFFU)]
    [DataRow(0x06C00018U)]
    [DataRow(0x04003FFDU)]
    [TestMethod]
    public void BltzDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Bltz>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }
    
    [TestCategory("Bne")]
    [DataRow(7, 28, 0x03F03FFF, 0x14FC3FFFU)]
    [DataRow(22, 12, 0x18, 0x16CC0018U)]
    [DataRow(0, 3, 0x03F03FFD, 0x14033FFDU)]
    [TestMethod]
    public void BneAssembly(int rs, int rt, int immediate, uint expected) {
        var instruction = new Bne {
            Rs = (byte)rs,
            Rt = (byte)rt,
            Offset = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Bne")]
    [DataRow(0x14FC3FFFU)]
    [DataRow(0x16CC0018U)]
    [DataRow(0x14033FFDU)]
    [TestMethod]
    public void BneDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Bne>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }

    [TestCategory("Lwcz")]
    [DataRow(1, 10, 0, 5, 0xC540_0005U)]
    [DataRow(2, 10, 0, 5, 0xC940_0005U)]
    [TestMethod]
    public void LwczAssembly(int coproc, int @base, int rt, int offset, uint expected)
    {
        Lwcz instruction = new() {
            Coprocessor = (byte)coproc,
            Base = (byte)@base,
            Ft = (byte)rt,
            Offset = (short)offset
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }
    
    [TestCategory("Lwcz")]
    [DataRow(0xC540_0005)]
    [DataRow(0xC940_0005)]
    [TestMethod]
    public void LwczDisassembly(uint instruction)
    {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Lwcz>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }
    
    [TestCategory("Swcz")]
    [DataRow(1, 10, 0, 5, 0xE540_0005)]
    [DataRow(2, 10, 0, 5, 0xE940_0005)]
    [TestMethod]
    public void SwczAssembly(int coproc, int @base, int rt, int offset, uint expected)
    {
        Swcz instruction = new() {
            Coprocessor = (byte)coproc,
            Base = (byte)@base,
            Rt = (byte)rt,
            Offset = (short)offset
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }
    
    [TestCategory("Swcz")]
    [DataRow(0xE540_0005U)]
    [DataRow(0xE940_0005U)]
    [TestMethod]
    public void SwczDisassembly(uint instruction) {
        IInstruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Swcz>(disassembled);
        Assert.AreEqual(instruction, disassembled.ConvertToInt());
    }
}
