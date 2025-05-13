﻿using System.Text.RegularExpressions;
using SAAE.Engine.Mips.Instructions;

namespace SAAE.Engine.Test.Mips;

[TestClass]
public class TypeITest {

    [TestCategory("Addi")]
    [TestMethod]
    public void AddiRegex() {
        var instruction = new Addi();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("addi $t0, $t1, 5"));
        Assert.IsTrue(regex.IsMatch("addi $t0, $t1, -5"));
        Assert.IsTrue(regex.IsMatch("addi $t0, $t1, 0xA"));
        Assert.IsFalse(regex.IsMatch("addi $t0, $t1, -0Xa"));
        Assert.IsFalse(regex.IsMatch("addi $t0, $t1, 0x"));
        Assert.IsFalse(regex.IsMatch("addi $t0, $t1, 0xG"));
        Assert.IsFalse(regex.IsMatch("addi $t0, $t1, -"));
        Assert.IsFalse(regex.IsMatch("addiu $t0, $t1, 5"));
    }

    [TestCategory("Addi")]
    [DataRow(8, 0, 0xF, 0x2008000F)]
    [DataRow(0, 10, 0x8F, 0x2140008F)]
    [DataRow(23,29, 0xFFE4, 0x23B7FFE4)]
    [DataTestMethod]
    public void AddiAssembly(int rt, int rs, int immediate, int expected) {
        var instruction = new Addi() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory(("Addiu"))]
    [TestMethod]
    public void AddiuRegex() {
        var instruction = new Addiu();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("addiu $t0, $t1, 5"));
        Assert.IsTrue(regex.IsMatch("addiu $t0, $t1, -5"));
        Assert.IsTrue(regex.IsMatch("addiu $t0, $t1, 0xA"));
        Assert.IsFalse(regex.IsMatch("addiu $t0, $t1, -0Xa"));
        Assert.IsFalse(regex.IsMatch("addiu $t0, $t1, 0x"));
        Assert.IsFalse(regex.IsMatch("addiu $t0, $t1, 0xG"));
        Assert.IsFalse(regex.IsMatch("addiu $t0, $t1, -"));
        Assert.IsFalse(regex.IsMatch("addi $t0, $t1, 5"));
    }

    [TestCategory("Addiu")]
    [DataRow(8, 0, 0xF, 0x2408000F)]
    [DataRow(0, 10, 0x8F, 0x2540008F)]
    [DataRow(23,29, 0x7FE4, 0x27B77FE4)]
    [DataTestMethod]
    public void AddiuAssembly(int rt, int rs, int immediate, int expected) {
        var instruction = new Addiu() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Slti")]
    [TestMethod]
    public void SltiRegex() { 
        var instruction = new Slti();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("slti $t0, $t1, 5"));
        Assert.IsTrue(regex.IsMatch("slti $t0, $t1, -5"));
        Assert.IsTrue(regex.IsMatch("slti $t0, $t1, 0xA"));
        Assert.IsFalse(regex.IsMatch("slti $t0, $t1, -0Xa"));
        Assert.IsFalse(regex.IsMatch("slti $t0, $t1, 0x"));
        Assert.IsFalse(regex.IsMatch("slti $t0, $t1, 0xG"));
        Assert.IsFalse(regex.IsMatch("slti $t0, $t1, -"));
        Assert.IsFalse(regex.IsMatch("sltiu $t0, $t1, 5"));
    }

    [TestCategory("Slti")]
    [DataRow(8, 0, 0xF, 0x2808000F)]
    [DataRow(0, 10, 0x8F, 0x2940008F)]
    [DataRow(23,29, 0xFFE4, 0x2BB7FFE4)]
    [DataTestMethod]
    public void SltiAssembly(int rt, int rs, int immediate, int expected) {
        var instruction = new Slti() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Sltiu")]
    [TestMethod]
    public void SltiuRegex() {
        var instruction = new Sltiu();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("sltiu $t0, $t1, 5"));
        Assert.IsTrue(regex.IsMatch("sltiu $t0, $t1, -5"));
        Assert.IsTrue(regex.IsMatch("sltiu $t0, $t1, 0xA"));
        Assert.IsFalse(regex.IsMatch("sltiu $t0, $t1, -0Xa"));
        Assert.IsFalse(regex.IsMatch("sltiu $t0, $t1, 0x"));
        Assert.IsFalse(regex.IsMatch("sltiu $t0, $t1, 0xG"));
        Assert.IsFalse(regex.IsMatch("sltiu $t0, $t1, -"));
        Assert.IsFalse(regex.IsMatch("slti $t0, $t1, 5"));
    }

    [TestCategory("Sltiu")]
    [DataRow(8, 0, 0xF, 0x2C08000F)]
    [DataRow(0, 10, 0x8F, 0x2D40008F)]
    [DataRow(23,29, 0xFFE4, 0x2FB7FFE4)]
    [DataTestMethod]
    public void SltiuAssembly(int rt, int rs, int immediate, int expected) {
        var instruction = new Sltiu() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Andi")]
    [TestMethod]
    public void AndiRegex() {
        var instruction = new Andi();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("andi $t0, $t1, 5"));
        Assert.IsTrue(regex.IsMatch("andi $t0, $t1, -5"));
        Assert.IsTrue(regex.IsMatch("andi $t0, $t1, 0xA"));
        Assert.IsFalse(regex.IsMatch("andi $t0, $t1, -0Xa"));
        Assert.IsFalse(regex.IsMatch("andi $t0, $t1, 0x"));
        Assert.IsFalse(regex.IsMatch("andi $t0, $t1, 0xG"));
        Assert.IsFalse(regex.IsMatch("andi $t0, $t1, -"));
        Assert.IsFalse(regex.IsMatch("addi $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("and $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("and $t0, $t1, 5"));
    }

    [TestCategory("Andi")]
    [DataRow(8, 0, 0xF, 0x3008000F)]
    [DataRow(0, 10, 0x8F, 0x3140008F)]
    /*[DataRow(23,29, 0xFFE4, 0x3C01FFFF)]*/ // andi with negative immediate is pseudo instruction of lui and ori
    [DataTestMethod]
    public void AndiAssembly(int rt, int rs, int immediate, int expected) {
        var instruction = new Andi() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Ori")]
    [TestMethod]
    public void OriRegex() {
        var instruction = new Ori();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("ori $t0, $t1, 5"));
        Assert.IsTrue(regex.IsMatch("ori $t0, $t1, -5"));
        Assert.IsTrue(regex.IsMatch("ori $t0, $t1, 0xA"));
        Assert.IsFalse(regex.IsMatch("ori $t0, $t1, -0Xa"));
        Assert.IsFalse(regex.IsMatch("ori $t0, $t1, 0x"));
        Assert.IsFalse(regex.IsMatch("ori $t0, $t1, 0xG"));
        Assert.IsFalse(regex.IsMatch("ori $t0, $t1, -"));
        Assert.IsFalse(regex.IsMatch("or $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("or $t0, $t1, 5"));
    }

    [TestCategory("Ori")]
    [DataRow(8, 0, 0xF, 0x3408000F)]
    [DataRow(0, 10, 0x8F, 0x3540008F)]
    [DataTestMethod]
    public void OriAssembly(int rt, int rs, int immediate, int expected) {
        var instruction = new Ori() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Xori")]
    [TestMethod]
    public void XoriRegex() {
        var instruction = new Xori();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("xori $t0, $t1, 5"));
        Assert.IsTrue(regex.IsMatch("xori $t0, $t1, -5"));
        Assert.IsTrue(regex.IsMatch("xori $t0, $t1, 0xA"));
        Assert.IsFalse(regex.IsMatch("xori $t0, $t1, -0Xa"));
        Assert.IsFalse(regex.IsMatch("xori $t0, $t1, 0x"));
        Assert.IsFalse(regex.IsMatch("xori $t0, $t1, 0xG"));
        Assert.IsFalse(regex.IsMatch("xori $t0, $t1, -"));
        Assert.IsFalse(regex.IsMatch("xor $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("xor $t0, $t1, 5"));
    }

    [TestCategory("Xori")]
    [DataRow(8, 0, 0xF, 0x3808000F)]
    [DataRow(0, 10, 0x8F, 0x3940008F)]
    [DataTestMethod]
    public void XoriAssembly(int rt, int rs, int immediate, int expected) {
        var instruction = new Xori() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Lb")]
    [TestMethod]
    public void LbRegex() {
        var instruction = new Lb();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("lb $s2, 5($k1)"));
        Assert.IsTrue(regex.IsMatch("lb $v1, -5($at)"));
        Assert.IsTrue(regex.IsMatch("lb $a2, 0xA($sp)"));
        Assert.IsFalse(regex.IsMatch("lb $t0, -0Xa($t1)"));
        Assert.IsFalse(regex.IsMatch("lb $t0, 0x($t1)"));
        Assert.IsFalse(regex.IsMatch("lb $t0, 0xG($t1)"));
        Assert.IsFalse(regex.IsMatch("lb $t0, -($t1)"));
        Assert.IsFalse(regex.IsMatch("lb $t0, $t1"));
    }

    [TestCategory("Lb")]
    [DataRow(1, 0xF, 11, 0x8161000F)]
    [DataRow(27, 0xFFF1, 28, 0x839BFFF1)]
    [DataRow(31, 0x1, 23, 0x82FF0001)]
    [DataTestMethod]
    public void LbAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Lb() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual((int)expected, instruction.ConvertToInt());
    }

    [TestCategory("Lbu")]
    [TestMethod]
    public void LbuRegex() {
        var instruction = new Lbu();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("lbu $s2, 5($k1)"));
        Assert.IsTrue(regex.IsMatch("lbu $v1, -5($at)"));
        Assert.IsTrue(regex.IsMatch("lbu $a2, 0xA($sp)"));
        Assert.IsFalse(regex.IsMatch("lbu $t0, -0Xa($t1)"));
        Assert.IsFalse(regex.IsMatch("lbu $t0, 0x($t1)"));
        Assert.IsFalse(regex.IsMatch("lbu $t0, 0xG($t1)"));
        Assert.IsFalse(regex.IsMatch("lbu $t0, -($t1)"));
        Assert.IsFalse(regex.IsMatch("lbu $t0, $t1"));
    }

    [TestCategory("Lbu")]
    [DataRow(1, 0xF, 11, 0x9161000F)]
    [DataRow(27, 0xFFF1, 28, 0x939BFFF1)]
    [DataRow(31, 0x1, 23, 0x92FF0001)]
    [DataTestMethod]
    public void LbuAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Lbu() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual((int)expected, instruction.ConvertToInt());
    }

    [TestCategory("Lh")]
    [TestMethod]
    public void LhRegex() {
        var instruction = new Lh();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("lh $s2, 5($k1)"));
        Assert.IsTrue(regex.IsMatch("lh $v1, -5($at)"));
        Assert.IsTrue(regex.IsMatch("lh $a2, 0xA($sp)"));
        Assert.IsFalse(regex.IsMatch("lh $t0, -0Xa($t1)"));
        Assert.IsFalse(regex.IsMatch("lh $t0, 0x($t1)"));
        Assert.IsFalse(regex.IsMatch("lh $t0, 0xG($t1)"));
        Assert.IsFalse(regex.IsMatch("lh $t0, -($t1)"));
        Assert.IsFalse(regex.IsMatch("lh $t0, $t1"));
    }

    [TestCategory("Lh")]
    [DataRow(1, 0xF, 11, 0x8561000F)]
    [DataRow(27, 0xFFF1, 28, 0x879BFFF1)]
    [DataRow(31, 0x1, 23, 0x86FF0001)]
    [DataTestMethod]
    public void LhAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Lh() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual((int)expected, instruction.ConvertToInt());
    }

    [TestCategory("Lhu")]
    [TestMethod]
    public void LhuRegex() {
        var instruction = new Lhu();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("lhu $s2, 5($k1)"));
        Assert.IsTrue(regex.IsMatch("lhu $v1, -5($at)"));
        Assert.IsTrue(regex.IsMatch("lhu $a2, 0xA($sp)"));
        Assert.IsFalse(regex.IsMatch("lhu $t0, -0Xa($t1)"));
        Assert.IsFalse(regex.IsMatch("lhu $t0, 0x($t1)"));
        Assert.IsFalse(regex.IsMatch("lhu $t0, 0xG($t1)"));
        Assert.IsFalse(regex.IsMatch("lhu $t0, -($t1)"));
        Assert.IsFalse(regex.IsMatch("lhu $t0, $t1"));
    }

    [TestCategory("Lhu")]
    [DataRow(1, 0xF, 11, 0x9561000F)]
    [DataRow(27, 0xFFF1, 28, 0x979BFFF1)]
    [DataRow(31, 0x1, 23, 0x96FF0001)]
    [DataTestMethod]
    public void LhuAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Lhu() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual((int)expected, instruction.ConvertToInt());
    }

    [TestCategory("Lui")]
    [TestMethod]
    public void LuiRegex() {
        var instruction = new Lui();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("lui $s2, 5"));
        Assert.IsTrue(regex.IsMatch("lui $s2, 0"));
        Assert.IsTrue(regex.IsMatch("lui $v1, -5"));
        Assert.IsTrue(regex.IsMatch("lui $a2, 0xA"));
        Assert.IsFalse(regex.IsMatch("lui $t0, -0Xa"));
        Assert.IsFalse(regex.IsMatch("lui $t0, 0x"));
        Assert.IsFalse(regex.IsMatch("lui $t0, 0xG"));
        Assert.IsFalse(regex.IsMatch("lui $t0, -"));
        Assert.IsFalse(regex.IsMatch("lui $t0, $t1"));
    }

    [TestCategory("Lui")]
    [DataRow(1, 0xF, 0x3C01000F)]
    [DataRow(27, 0, 0x3C1B0000)]
    [DataRow(31, 0xFA, 0x3C1F00FA)]
    [DataTestMethod]
    public void LuiAssembly(int rt, int immediate, int expected) {
        var instruction = new Lui() {
            Rt = (byte)rt,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    // LW
    [TestCategory("Lw")]
    [TestMethod]
    public void LwRegex() {
        var instruction = new Lw();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("lw $s2, 5($k1)"));
        Assert.IsTrue(regex.IsMatch("lw $v1, -5($at)"));
        Assert.IsTrue(regex.IsMatch("lw $a2, 0xA($sp)"));
        Assert.IsTrue(regex.IsMatch("lw $a2, 0XA($sp)"));
        Assert.IsFalse(regex.IsMatch("lw $t0, -0Xa($t1)"));
        Assert.IsFalse(regex.IsMatch("lw $t0, 0x($t1)"));
        Assert.IsFalse(regex.IsMatch("lw $t0, 0xG($t1)"));
        Assert.IsFalse(regex.IsMatch("lw $t0, -($t1)"));
        Assert.IsFalse(regex.IsMatch("lw $t0, $t1"));
    }

    [TestCategory("Lw")]
    [DataRow(1, 0xF, 11, 0x8D61000F)]
    [DataRow(27, 0x7FF1, 28, 0x8F9B7FF1)]
    [DataRow(31, 0x1, 23, 0x8EFF0001)]
    [DataTestMethod]
    public void LwAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Lw() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual((int)expected, instruction.ConvertToInt());
    }

    [TestCategory("Sb")]
    [TestMethod]
    public void SbRegex() {
        var instruction = new Sb();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("sb $s2, 5($k1)"));
        Assert.IsTrue(regex.IsMatch("sb $v1, -5($at)"));
        Assert.IsTrue(regex.IsMatch("sb $a2, 0xA($sp)"));
        Assert.IsTrue(regex.IsMatch("sb $a2, 0XA($sp)"));
        Assert.IsFalse(regex.IsMatch("sb $t0, -0Xa($t1)"));
        Assert.IsFalse(regex.IsMatch("sb $t0, 0x($t1)"));
        Assert.IsFalse(regex.IsMatch("sb $t0, 0xG($t1)"));
        Assert.IsFalse(regex.IsMatch("sb $t0, -($t1)"));
        Assert.IsFalse(regex.IsMatch("sb $t0, $t1"));
    }

    [TestCategory("Sb")]
    [DataRow(1, 0xF, 11, 0xA161000F)]
    [DataRow(27, 0x7FF1, 28, 0xA39B7FF1)]
    [DataRow(31, 0x1, 23, 0xA2FF0001)]
    [DataTestMethod]
    public void SbAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Sb() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual((int)expected, instruction.ConvertToInt());
    }

    [TestCategory("Sh")]
    [TestMethod]
    public void ShRegex() {
        var instruction = new Sh();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("sh $s2, 5($k1)"));
        Assert.IsTrue(regex.IsMatch("sh $v1, -5($at)"));
        Assert.IsTrue(regex.IsMatch("sh $a2, 0xA($sp)"));
        Assert.IsTrue(regex.IsMatch("sh $a2, 0XA($sp)"));
        Assert.IsFalse(regex.IsMatch("sh $t0, -0Xa($t1)"));
        Assert.IsFalse(regex.IsMatch("sh $t0, 0x($t1)"));
        Assert.IsFalse(regex.IsMatch("sh $t0, 0xG($t1)"));
        Assert.IsFalse(regex.IsMatch("sh $t0, -($t1)"));
        Assert.IsFalse(regex.IsMatch("sh $t0, $t1"));
    }

    [TestCategory("Sh")]
    [DataRow(1, 0xF, 11, 0xA561000F)]
    [DataRow(27, 0x7FF1, 28, 0xA79B7FF1)]
    [DataRow(31, 0x1, 23, 0xA6FF0001)]
    [DataTestMethod]
    public void ShAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Sh() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual((int)expected, instruction.ConvertToInt());
    }

    [TestCategory("Sw")]
    [TestMethod]
    public void SwRegex() {
        var instruction = new Sw();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("sw $s2, 5($k1)"));
        Assert.IsTrue(regex.IsMatch("sw $v1, -5($at)"));
        Assert.IsTrue(regex.IsMatch("sw $a2, 0xA($sp)"));
        Assert.IsTrue(regex.IsMatch("sw $a2, 0XA($sp)"));
        Assert.IsFalse(regex.IsMatch("sw $t0, -0Xa($t1)"));
        Assert.IsFalse(regex.IsMatch("sw $t0, 0x($t1)"));
        Assert.IsFalse(regex.IsMatch("sw $t0, 0xG($t1)"));
        Assert.IsFalse(regex.IsMatch("sw $t0, -($t1)"));
        Assert.IsFalse(regex.IsMatch("sw $t0, $t1"));
    }

    [TestCategory("Sw")]
    [DataRow(1, 0xF, 11, 0xAD61000F)]
    [DataRow(27, 0x7FF1, 28, 0xAF9B7FF1)]
    [DataRow(31, 0x1, 23, 0xAEFF0001)]
    [DataTestMethod]
    public void SwAssembly(int rt, int immediate, int rs, uint expected) {
        var instruction = new Sw() {
            Rt = (byte)rt,
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual((int)expected, instruction.ConvertToInt());
    }

    [TestCategory("Beq")]
    [TestMethod]
    public void BeqRegex() {
        var instruction = new Beq();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("beq $s2, $k1, 5"));
        Assert.IsTrue(regex.IsMatch("beq $v1, $at, -5"));
        Assert.IsTrue(regex.IsMatch("beq $a2, $sp, 0xA"));
        Assert.IsFalse(regex.IsMatch("beq $t0, $t1, -0Xa"));
        Assert.IsFalse(regex.IsMatch("beq $t0, $t1, 0x"));
        Assert.IsFalse(regex.IsMatch("beq $t0, $t1, 0xG"));
        Assert.IsFalse(regex.IsMatch("beq $t0, $t1, -"));
        Assert.IsFalse(regex.IsMatch("beq $t0, $t1"));
    }

    [TestCategory("Beq")]
    [DataRow(7, 28, 0x03F03FFF, 0x10FC3FFF)]
    [DataRow(22, 12, 0x18, 0x12CC0018)]
    [DataRow(0, 3, 0x03F03FFD, 0x10033FFD)]
    [DataRow(10, 8, 0x2, 0x11480002)]
    [DataTestMethod]
    public void BeqAssembly(int rs, int rt, int immediate, int expected) {
        var instruction = new Beq() {
            Rs = (byte)rs,
            Rt = (byte)rt,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Beq")]
    [DataRow("beq $t2, $t0, 0x2", 10, 8, 0x2)]
    [DataTestMethod]
    public void BeqPopulate(string line, int rs, int rt, int immediate) {
        var instruction = new Beq();
        instruction.PopulateFromLine(line);
        Assert.AreEqual(rs, instruction.Rs);
        Assert.AreEqual(rt, instruction.Rt);
        Assert.AreEqual(immediate, instruction.Immediate);
    }

    [TestCategory("Bgez")]
    [TestMethod]
    public void BgezRegex() {
        var instruction = new Bgez();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("bgez $s2, 5"));
        Assert.IsTrue(regex.IsMatch("bgez $v1, -5"));
        Assert.IsTrue(regex.IsMatch("bgez $a2, 0xA"));
        Assert.IsFalse(regex.IsMatch("bgez $t0, -0Xa"));
        Assert.IsFalse(regex.IsMatch("bgez $t0, 0x"));
        Assert.IsFalse(regex.IsMatch("bgez $t0, 0xG"));
        Assert.IsFalse(regex.IsMatch("bgez $t0, -"));
        Assert.IsFalse(regex.IsMatch("bgez $t0, $t1"));
    }

    [TestCategory("Bgez")]
    [DataRow(7, 0x03F03FFF, 0x04E13FFF)]
    [DataRow(22, 0x18, 0x06C10018)]
    [DataRow(0, 0x03F03FFD, 0x04013FFD)]
    [DataTestMethod]
    public void BgezAssembly(int rs, int immediate, int expected) {
        var instruction = new Bgez() {
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Bgtz")]
    [TestMethod]
    public void BgtzRegex() {
        var instruction = new Bgtz();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("bgtz $s2, 5"));
        Assert.IsTrue(regex.IsMatch("bgtz $v1, -5"));
        Assert.IsTrue(regex.IsMatch("bgtz $a2, 0xA"));
        Assert.IsFalse(regex.IsMatch("bgtz $t0, -0Xa"));
        Assert.IsFalse(regex.IsMatch("bgtz $t0, 0x"));
        Assert.IsFalse(regex.IsMatch("bgtz $t0, 0xG"));
        Assert.IsFalse(regex.IsMatch("bgtz $t0, -"));
        Assert.IsFalse(regex.IsMatch("bgtz $t0, $t1"));
    }

    [TestCategory("Bgtz")]
    [DataRow(7, 0x03F03FFF, 0x1CE03FFF)]
    [DataRow(22, 0x18, 0x1EC00018)]
    [DataRow(0, 0x03F03FFD, 0x1C003FFD)]
    [DataTestMethod]
    public void BgtzAssembly(int rs, int immediate, int expected) {
        var instruction = new Bgtz() {
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Blez")]
    [TestMethod]
    public void BlezRegex() {
        var instruction = new Blez();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("blez $s2, 5"));
        Assert.IsTrue(regex.IsMatch("blez $v1, -5"));
        Assert.IsTrue(regex.IsMatch("blez $a2, 0xA"));
        Assert.IsFalse(regex.IsMatch("blez $t0, -0Xa"));
        Assert.IsFalse(regex.IsMatch("blez $t0, 0x"));
        Assert.IsFalse(regex.IsMatch("blez $t0, 0xG"));
        Assert.IsFalse(regex.IsMatch("blez $t0, -"));
        Assert.IsFalse(regex.IsMatch("blez $t0, $t1"));
    }

    [TestCategory("Blez")]
    [DataRow(7, 0x03F03FFF, 0x18E03FFF)]
    [DataRow(22, 0x18, 0x1AC00018)]
    [DataRow(0, 0x03F03FFD, 0x18003FFD)]
    [DataTestMethod]
    public void BlezAssembly(int rs, int immediate, int expected) {
        var instruction = new Blez() {
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Bltz")]
    [TestMethod]
    public void BltzRegex() {
        var instruction = new Bltz();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("bltz $s2, 5"));
        Assert.IsTrue(regex.IsMatch("bltz $v1, -5"));
        Assert.IsTrue(regex.IsMatch("bltz $a2, 0xA"));
        Assert.IsFalse(regex.IsMatch("bltz $t0, -0Xa"));
        Assert.IsFalse(regex.IsMatch("bltz $t0, 0x"));
        Assert.IsFalse(regex.IsMatch("bltz $t0, 0xG"));
        Assert.IsFalse(regex.IsMatch("bltz $t0, -"));
        Assert.IsFalse(regex.IsMatch("bltz $t0, $t1"));
    }

    [TestCategory("Bltz")]
    [DataRow(7, 0x03F03FFF, 0x04E03FFF)]
    [DataRow(22, 0x18, 0x06C00018)]
    [DataRow(0, 0x03F03FFD, 0x04003FFD)]
    [DataTestMethod]
    public void BltzAssembly(int rs, int immediate, int expected) {
        var instruction = new Bltz() {
            Rs = (byte)rs,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }

    [TestCategory("Bne")]
    [TestMethod]
    public void BneRegex() {
        var instruction = new Bne();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("bne $s2, $k1, 5"));
        Assert.IsTrue(regex.IsMatch("bne $v1, $at, -5"));
        Assert.IsTrue(regex.IsMatch("bne $a2, $sp, 0xA"));
        Assert.IsFalse(regex.IsMatch("bne $t0, $t1, -0Xa"));
        Assert.IsFalse(regex.IsMatch("bne $t0, $t1, 0x"));
        Assert.IsFalse(regex.IsMatch("bne $t0, $t1, 0xG"));
        Assert.IsFalse(regex.IsMatch("bne $t0, $t1, -"));
        Assert.IsFalse(regex.IsMatch("bne $t0, $t1"));
    }

    [TestCategory("Bne")]
    [DataRow(7, 28, 0x03F03FFF, 0x14FC3FFF)]
    [DataRow(22, 12, 0x18, 0x16CC0018)]
    [DataRow(0, 3, 0x03F03FFD, 0x14033FFD)]
    [DataTestMethod]
    public void BneAssembly(int rs, int rt, int immediate, int expected) {
        var instruction = new Bne() {
            Rs = (byte)rs,
            Rt = (byte)rt,
            Immediate = (short)immediate
        };
        Assert.AreEqual(expected, instruction.ConvertToInt());
    }


        


}
