namespace SAAE.Engine.Test.Mips;

[TestClass]
public class InstructionsTestTypeR
{
    [TestCategory("Add")]
    [TestMethod("Test Add Regex")]
    public void AddRegex()
    {
        var instruction = new Engine.Mips.Instructions.Add();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("add $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("add $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("add $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("addu $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("add $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Add")]
    [DataRow(8, 9, 10, 0x012A4020)]
    [DataRow(0, 9, 31, 0x013F0020)]
    [DataRow(23, 29, 26, 0x03BAB820)]
    [DataTestMethod("Test assembling")]
    public void AddAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.Add {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Addu")]
    [TestMethod("Test Addu Regex")]
    public void AdduRegex() {
        var instruction = new Engine.Mips.Instructions.Addu();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("addu $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("addu $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("add $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("addu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("addu $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Addu")]
    [DataRow(8, 9, 10, 0x012A4021)]
    [DataRow(0, 9, 31, 0x013F0021)]
    [DataRow(23, 29, 26, 0x03BAB821)]
    [DataTestMethod("Test assembling")]
    public void AdduAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.Addu {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mul")]
    [TestMethod("Test Mul Regex")]
    public void MulRegex() {
        var instruction = new Engine.Mips.Instructions.Mul();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("mul $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("mul $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("mul $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("mult $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("mul $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Mul")]
    [DataRow(8, 9, 10, 0x712A4002)]
    [DataRow(0, 9, 31, 0x713F0002)]
    [DataRow(23, 29, 26, 0x73BAB802)]
    [DataTestMethod("Test assembling")]
    public void MulAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.Mul {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Slt")]
    [TestMethod("Test Slt Regex")]
    public void SltRegex() {
        var instruction = new Engine.Mips.Instructions.Slt();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("slt $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("slt $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("slt $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("slt $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Slt")]
    [DataRow(8, 9, 10, 0x012A402A)]
    [DataRow(0, 9, 31, 0x013F002A)]
    [DataRow(23, 29, 26, 0x03BAB82A)]
    [DataTestMethod("Test assembling")]
    public void SltAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.Slt {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sltu")]
    [TestMethod("Test Sltu Regex")]
    public void SltuRegex() {
        var instruction = new Engine.Mips.Instructions.Sltu();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("sltu $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("sltu $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("slt $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("sltu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("sltu $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Sltu")]
    [DataRow(8, 9, 10, 0x012A402B)]
    [DataRow(0, 9, 31, 0x013F002B)]
    [DataRow(23, 29, 26, 0x03BAB82B)]
    [DataTestMethod("Test assembling")]
    public void SltuAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.Sltu {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sub")]
    [TestMethod("Test Sub Regex")]
    public void SubRegex() {
        var instruction = new Engine.Mips.Instructions.Sub();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("sub $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("sub $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("subu $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("sub $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("sub $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Sub")]
    [DataRow(8, 9, 10, 0x012A4022)]
    [DataRow(0, 9, 31, 0x013F0022)]
    [DataRow(23, 29, 26, 0x03BAB822)]
    [DataTestMethod("Test assembling")]
    public void SubAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.Sub {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Subu")]
    [TestMethod("Test Subu Regex")]
    public void SubuRegex() {
        var instruction = new Engine.Mips.Instructions.Subu();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("subu $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("subu $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("subu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("sub $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("subu $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Subu")]
    [DataRow(8, 9, 10, 0x012A4023)]
    [DataRow(0, 9, 31, 0x013F0023)]
    [DataRow(23, 29, 26, 0x03BAB823)]
    [DataTestMethod("Test assembling")]
    public void SubuAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.Subu {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("And")]
    [TestMethod("Test And Regex")]
    public void AndRegex() {
        var instruction = new Engine.Mips.Instructions.And();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("and $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("and $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("and $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("andi $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("and $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("And")]
    [DataRow(8, 9, 10, 0x012A4024)]
    [DataRow(0, 9, 31, 0x013F0024)]
    [DataRow(23, 29, 26, 0x03BAB824)]
    [DataTestMethod("Test assembling")]
    public void AndAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.And {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Nor")]
    [TestMethod("Test Nor Regex")]
    public void NorRegex() {
        var instruction = new Engine.Mips.Instructions.Nor();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("nor $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("nor $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("nor $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("nor $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Nor")]
    [DataRow(8, 9, 10, 0x012A4027)]
    [DataRow(0, 9, 31, 0x013F0027)]
    [DataRow(23, 29, 26, 0x03BAB827)]
    [DataTestMethod("Test assembling")]
    public void NorAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.Nor {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Or")]
    [TestMethod("Test Or Regex")]
    public void OrRegex() {
        var instruction = new Engine.Mips.Instructions.Or();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("or $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("or $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("or $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("ori $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("or $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Or")]
    [DataRow(8, 9, 10, 0x012A4025)]
    [DataRow(0, 9, 31, 0x013F0025)]
    [DataRow(23, 29, 26, 0x03BAB825)]
    [DataTestMethod("Test assembling")]
    public void OrAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.Or {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Xor")]
    [TestMethod("Test Xor Regex")]
    public void XorRegex() {
        var instruction = new Engine.Mips.Instructions.Xor();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("xor $t0, $zero, $t2"));
        Assert.IsTrue(regex.IsMatch("xor $s0, $t1, $k1"));
        Assert.IsTrue(regex.IsMatch("xor $t0, $t1, $gp"));
        Assert.IsFalse(regex.IsMatch("xor $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("xor $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("xori $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("xor $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Xor")]
    [DataRow(8, 9, 10, 0x012A4026)]
    [DataRow(0, 9, 31, 0x013F0026)]
    [DataRow(23, 29, 26, 0x03BAB826)]
    [DataTestMethod("Test assembling")]
    public void XorAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Engine.Mips.Instructions.Xor {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sll")]
    [TestMethod("Test Sll Regex")]
    public void SllRegex() {
        var instruction = new Engine.Mips.Instructions.Sll();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("sll $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("sll $t0, $t1, 5, $t3"));
        Assert.IsFalse(regex.IsMatch("sll $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("sll $t0, $t1, $k0"));
        Assert.IsFalse(regex.IsMatch("sll $t0, $t1, 5, $t3, $t4"));
    }

    [TestCategory("Sll")]
    [DataRow(8, 9, 1, 0x00094040)]
    [DataRow(0, 9, 10, 0x00090280)]
    [DataRow(23, 29, 5, 0X001DB940)]
    [DataTestMethod("Test assembling")]
    public void SllAssembly(int rd, int rt, int shamt, int result) {
        var instruction = new Engine.Mips.Instructions.Sll {
            Rd = (byte)rd,
            Rt = (byte)rt,
            ShiftAmount = (byte)shamt
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sllv")]
    [TestMethod("Test Sllv Regex")]
    public void SllvRegex() {
        var instruction = new Engine.Mips.Instructions.Sllv();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("sllv $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("sllv $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("sllv $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("sll $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("sllv $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Sllv")]
    [DataRow(8, 9, 10, 0x01494004)]
    [DataRow(0, 9, 31, 0x03E90004)]
    [DataRow(23, 29, 26, 0x035DB804)]
    [DataTestMethod("Test assembling")]
    public void SllvAssembly(int rd, int rt, int rs, int result) {
        var instruction = new Engine.Mips.Instructions.Sllv {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt(), $"EXP:{Convert.ToHexString(BitConverter.GetBytes(result))}\nREA:{Convert.ToHexString(BitConverter.GetBytes(instruction.ConvertToInt()))}");
    }

    [TestCategory("Sra")]
    [TestMethod("Test Sra Regex")]
    public void SraRegex() {
        var instruction = new Engine.Mips.Instructions.Sra();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("sra $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("sra $t0, $t1, 5, $t3"));
        Assert.IsFalse(regex.IsMatch("sra $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("sra $t0, $t1, $k0"));
        Assert.IsFalse(regex.IsMatch("srav $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("srav $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("sra $t0, $t1, 5, $t3, $t4"));
    }

    [TestCategory("Sra")]
    [DataRow(8, 9, 1, 0x00094043)]
    [DataRow(0, 9, 10, 0x00090283)]
    [DataRow(23, 29, 5, 0x001DB943)]
    [DataTestMethod("Test assembling")]
    public void SraAssembly(int rd, int rt, int shamt, int result) {
        var instruction = new Engine.Mips.Instructions.Sra {
            Rd = (byte)rd,
            Rt = (byte)rt,
            ShiftAmount = (byte)shamt
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Srav")]
    [TestMethod("Test Srav Regex")]
    public void SravRegex() {
        var instruction = new Engine.Mips.Instructions.Srav();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("srav $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("srav $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("srav $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("sra $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("sra $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("srav $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("srav $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Srav")]
    [DataRow(8, 9, 10, 0x01494007)]
    [DataRow(0, 9, 31, 0x03E90007)]
    [DataRow(23, 29, 26, 0x035DB807)]
    [DataTestMethod("Test assembling")]
    public void SravAssembly(int rd, int rt, int rs, int result) {
        var instruction = new Engine.Mips.Instructions.Srav {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Srl")]
    [TestMethod("Test Srl Regex")]
    public void SrlRegex() {
        var instruction = new Engine.Mips.Instructions.Srl();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("srl $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("srl $t0, $t1, 5, $t3"));
        Assert.IsFalse(regex.IsMatch("srl $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("srl $t0, $t1, $k0"));
        Assert.IsFalse(regex.IsMatch("srlv $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("srlv $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("srl $t0, $t1, 5, $t3, $t4"));
    }

    [TestCategory("Srl")]
    [DataRow(8, 9, 1, 0x00094042)]
    [DataRow(0, 9, 10, 0x00090282)]
    [DataRow(23, 29, 5, 0x001DB942)]
    [DataTestMethod("Test assembling")]
    public void SrlAssembly(int rd, int rt, int shamt, int result) {
        var instruction = new Engine.Mips.Instructions.Srl {
            Rd = (byte)rd,
            Rt = (byte)rt,
            ShiftAmount = (byte)shamt
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Srlv")]
    [TestMethod("Test Srlv Regex")]
    public void SrlvRegex() {
        var instruction = new Engine.Mips.Instructions.Srlv();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("srlv $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("srlv $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("srlv $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("srl $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("srl $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("srlv $t0, $t1, 5"));
        Assert.IsFalse(regex.IsMatch("srlv $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Srlv")]
    [DataRow(8, 9, 10, 0x01494006)]
    [DataRow(0, 9, 31, 0x03E90006)]
    [DataRow(23, 29, 26, 0x035DB806)]
    [DataTestMethod("Test assembling")]
    public void SrlvAssembly(int rd, int rt, int rs, int result) {
        var instruction = new Engine.Mips.Instructions.Srlv {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }


}