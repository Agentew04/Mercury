using SAAE.Engine.Mips.Instructions;
using System.Text.RegularExpressions;
using SAAE.Engine.Mips;

namespace SAAE.Engine.Test.Mips;

[TestClass]
public class TypeRTest {

    [TestCategory("Add")]
    [TestMethod("Test Add Regex")]
    public void AddRegex()
    {
        var instruction = new Add();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Add {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }
    
    [TestCategory("Add")]
    [DataRow((uint)0x012A4020)]
    [DataRow((uint)0x013F0020)]
    [DataRow((uint)0x03BAB820)]
    [DataTestMethod("Test assembling")]
    public void AddDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Add>(disassembled);
    }

    [TestCategory("Addu")]
    [TestMethod("Test Addu Regex")]
    public void AdduRegex() {
        var instruction = new Addu();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Addu {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }
    
    [TestCategory("Addu")]
    [DataRow((uint)0x012A4021)]
    [DataRow((uint)0x013F0021)]
    [DataRow((uint)0x03BAB821)]
    [DataTestMethod("Test assembling")]
    public void AdduDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Addu>(disassembled);
    }

    [TestCategory("Mul")]
    [TestMethod("Test Mul Regex")]
    public void MulRegex() {
        var instruction = new Mul();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Mul {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }
    
    [TestCategory("Mul")]
    [DataRow((uint)0x712A4002)]
    [DataRow((uint)0x713F0002)]
    [DataRow((uint)0x73BAB802)]
    [DataTestMethod("Test assembling")]
    public void MulDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mul>(disassembled);
    }
    

    [TestCategory("Slt")]
    [TestMethod("Test Slt Regex")]
    public void SltRegex() {
        var instruction = new Slt();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Slt {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }
    
    [TestCategory("Slt")]
    [DataRow((uint)0x012A402A)]
    [DataRow((uint)0x013F002A)]
    [DataRow((uint)0x03BAB82A)]
    [DataTestMethod("Test disassembling")]
    public void SltDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Slt>(disassembled);
    }

    [TestCategory("Sltu")]
    [TestMethod("Test Sltu Regex")]
    public void SltuRegex() {
        var instruction = new Sltu();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Sltu {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sltu")]
    [DataRow((uint)0x012A402B)]
    [DataRow((uint)0x013F002B)]
    [DataRow((uint)0x03BAB82B)]
    [DataTestMethod("Test disassembling")]
    public void SltuDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sltu>(disassembled);
    }

    [TestCategory("Sub")]
    [TestMethod("Test Sub Regex")]
    public void SubRegex() {
        var instruction = new Sub();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Sub {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sub")]
    [DataRow((uint)0x012A4022)]
    [DataRow((uint)0x013F0022)]
    [DataRow((uint)0x03BAB822)]
    [DataTestMethod("Test disassembling")]
    public void SubDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sub>(disassembled);
    }

    [TestCategory("Subu")]
    [TestMethod("Test Subu Regex")]
    public void SubuRegex() {
        var instruction = new Subu();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Subu {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Subu")]
    [DataRow((uint)0x012A4023)]
    [DataRow((uint)0x013F0023)]
    [DataRow((uint)0x03BAB823)]
    [DataTestMethod("Test disassembling")]
    public void SubuDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Subu>(disassembled);
    }

    [TestCategory("And")]
    [TestMethod("Test And Regex")]
    public void AndRegex() {
        var instruction = new And();
        Regex? regex = instruction.GetRegularExpression();
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
    [DataTestMethod("Test disassembling")]
    public void AndDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<And>(disassembled);
    }

    [TestCategory("Nor")]
    [TestMethod("Test Nor Regex")]
    public void NorRegex() {
        var instruction = new Nor();
        Regex? regex = instruction.GetRegularExpression();
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
    [DataTestMethod("Test disassembling")]
    public void NorDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Nor>(disassembled);
    }

    [TestCategory("Or")]
    [TestMethod("Test Or Regex")]
    public void OrRegex() {
        var instruction = new Or();
        Regex? regex = instruction.GetRegularExpression();
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
    [DataRow(23, 26, 1, 0x0341B825)]
    [DataTestMethod("Test assembling")]
    public void OrAssembly(int rd, int rs, int rt, int result) {
        var instruction = new Or {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Or")]
    [DataRow((uint)0x012A4025)]
    [DataRow((uint)0x013F0025)]
    [DataRow((uint)0x03BAB825)]
    [DataRow((uint)0x0341B825)]
    [DataTestMethod("Test disassembling")]
    public void OrDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Or>(disassembled);
    }

    [TestCategory("Xor")]
    [TestMethod("Test Xor Regex")]
    public void XorRegex() {
        var instruction = new Xor();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Xor {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Xor")]
    [DataRow((uint)0x012A4026)]
    [DataRow((uint)0x013F0026)]
    [DataRow((uint)0x03BAB826)]
    [DataTestMethod("Test disassembling")]
    public void XorDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Xor>(disassembled);
    }

    [TestCategory("Sll")]
    [TestMethod("Test Sll Regex")]
    public void SllRegex() {
        var instruction = new Sll();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Sll {
            Rd = (byte)rd,
            Rt = (byte)rt,
            ShiftAmount = (byte)shamt
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sll")]
    [DataRow((uint)0x00094040)]
    [DataRow((uint)0x00090280)]
    [DataRow((uint)0x001DB940)]
    [DataTestMethod("Test disassembling")]
    public void SllDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sll>(disassembled);
    }

    [TestCategory("Sllv")]
    [TestMethod("Test Sllv Regex")]
    public void SllvRegex() {
        var instruction = new Sllv();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Sllv {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt(), $"EXP:{Convert.ToHexString(BitConverter.GetBytes(result))}\nREA:{Convert.ToHexString(BitConverter.GetBytes(instruction.ConvertToInt()))}");
    }

    [TestCategory("Sllv")]
    [DataRow((uint)0x01494004)]
    [DataRow((uint)0x03E90004)]
    [DataRow((uint)0x035DB804)]
    [DataTestMethod("Test disassembling")]
    public void SllvDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sllv>(disassembled);
    }

    [TestCategory("Sra")]
    [TestMethod("Test Sra Regex")]
    public void SraRegex() {
        var instruction = new Sra();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Sra {
            Rd = (byte)rd,
            Rt = (byte)rt,
            ShiftAmount = (byte)shamt
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Sra")]
    [DataRow((uint)0x00094043)]
    [DataRow((uint)0x00090283)]
    [DataRow((uint)0x001DB943)]
    [DataTestMethod("Test disassembling")]
    public void SraDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Sra>(disassembled);
    }

    [TestCategory("Srav")]
    [TestMethod("Test Srav Regex")]
    public void SravRegex() {
        var instruction = new Srav();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Srav {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Srav")]
    [DataRow((uint)0x01494007)]
    [DataRow((uint)0x03E90007)]
    [DataRow((uint)0x035DB807)]
    [DataTestMethod("Test disassembling")]
    public void SravDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Srav>(disassembled);
    }

    [TestCategory("Srl")]
    [TestMethod("Test Srl Regex")]
    public void SrlRegex() {
        var instruction = new Srl();
        Regex? regex = instruction.GetRegularExpression();
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
    [DataTestMethod("Test disassembling")]
    public void SrlDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Srl>(disassembled);
    }

    [TestCategory("Srlv")]
    [TestMethod("Test Srlv Regex")]
    public void SrlvRegex() {
        var instruction = new Srlv();
        Regex? regex = instruction.GetRegularExpression();
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
        var instruction = new Srlv {
            Rd = (byte)rd,
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Srlv")]
    [DataRow((uint)0x01494006)]
    [DataRow((uint)0x03E90006)]
    [DataRow((uint)0x035DB806)]
    [DataTestMethod("Test disassembling")]
    public void SrlvDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Srlv>(disassembled);
    }

    [TestCategory("Div")]
    [TestMethod("Test Div Regex")]
    public void DivRegex() {
        var instruction = new Div();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("div $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("div $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("div $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("div $t0"));
        Assert.IsFalse(regex.IsMatch("divu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("div $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Div")]
    [DataRow(8, 9, 0x0109001A)]
    [DataRow(0, 10, 0x000A001A)]
    [DataRow(23, 26, 0x02FA001A)]
    [DataTestMethod("Test assembling")]
    public void DivAssembly(int rs, int rt, int result) {
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
    [DataTestMethod("Test disassembling")]
    public void DivDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Div>(disassembled);
    }

    [TestCategory("Divu")]
    [TestMethod("Test Divu Regex")]
    public void DivuRegex() {
        var instruction = new Divu();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("divu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("divu $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("divu $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("divu $t0"));
        Assert.IsFalse(regex.IsMatch("div $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("divu $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Divu")]
    [DataRow(8, 9, 0x0109001B)]
    [DataRow(0, 10, 0x000A001B)]
    [DataRow(23, 26, 0x02FA001B)]
    [DataTestMethod("Test assembling")]
    public void DivuAssembly(int rs, int rt, int result) {
        var instruction = new Divu {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Divu")]
    [DataRow((uint)0x0109001B)]
    [DataRow((uint)0x000A001B)]
    [DataRow((uint)0x02FA001B)]
    [DataTestMethod("Test disassembling")]
    public void DivuDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Divu>(disassembled);
    }

    [TestCategory("Madd")]
    [TestMethod("Test Madd Regex")]
    public void MaddRegex() {
        var instruction = new Madd();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("madd $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("madd $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("madd $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("madd $t0"));
        Assert.IsFalse(regex.IsMatch("maddu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("madd $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Madd")]
    [DataRow(8, 9, 0x71090000)]
    [DataRow(0, 10, 0x700A0000)]
    [DataRow(23, 26, 0x72FA0000)]
    [DataTestMethod("Test assembling")]
    public void MaddAssembly(int rs, int rt, int result) {
        var instruction = new Madd {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Madd")]
    [DataRow((uint)0x71090000)]
    [DataRow((uint)0x700A0000)]
    [DataRow((uint)0x72FA0000)]
    [DataTestMethod("Test disassembling")]
    public void MaddDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Madd>(disassembled);
    }

    [TestCategory("Maddu")]
    [TestMethod("Test Maddu Regex")]
    public void MadduRegex() {
        var instruction = new Maddu();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("maddu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("maddu $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("maddu $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("maddu $t0"));
        Assert.IsFalse(regex.IsMatch("madd $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("maddu $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Maddu")]
    [DataRow(8, 9, 0x71090001)]
    [DataRow(0, 10, 0x700A0001)]
    [DataRow(23, 26, 0x72FA0001)]
    [DataTestMethod("Test assembling")]
    public void MadduAssembly(int rs, int rt, int result) {
        var instruction = new Maddu {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Maddu")]
    [DataRow((uint)0x71090001)]
    [DataRow((uint)0x700A0001)]
    [DataRow((uint)0x72FA0001)]
    [DataTestMethod("Test disassembling")]
    public void MadduDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Maddu>(disassembled);
    }

    [TestCategory("Msub")]
    [TestMethod("Test Msub Regex")]
    public void MsubRegex() {
        var instruction = new Msub();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("msub $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("msub $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("msub $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("msub $t0"));
        Assert.IsFalse(regex.IsMatch("msubu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("msub $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Msub")]
    [DataRow(8, 9, 0x71090004)]
    [DataRow(0, 10, 0x700A0004)]
    [DataRow(23, 26, 0x72FA0004)]
    [DataTestMethod("Test assembling")]
    public void MsubAssembly(int rs, int rt, int result) {
        var instruction = new Msub {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Msub")]
    [DataRow((uint)0x71090004)]
    [DataRow((uint)0x700A0004)]
    [DataRow((uint)0x72FA0004)]
    [DataTestMethod("Test disassembling")]
    public void MsubDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Msub>(disassembled);
    }

    [TestCategory("Msubu")]
    [TestMethod("Test Msubu Regex")]
    public void MsubuRegex() {
        var instruction = new Msubu();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("msubu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("msubu $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("msubu $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("msubu $t0"));
        Assert.IsFalse(regex.IsMatch("msub $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("msubu $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Msubu")]
    [DataRow(8, 9, 0x71090005)]
    [DataRow(0, 10, 0x700A0005)]
    [DataRow(23, 26, 0x72FA0005)]
    [DataTestMethod("Test assembling")]
    public void MsubuAssembly(int rs, int rt, int result) {
        var instruction = new Msubu {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Msubu")]
    [DataRow((uint)0x71090005)]
    [DataRow((uint)0x700A0005)]
    [DataRow((uint)0x72FA0005)]
    [DataTestMethod("Test disassembling")]
    public void MsubuDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Msubu>(disassembled);
    }

    [TestCategory("Mult")]
    [TestMethod("Test Mult Regex")]
    public void MultRegex() {
        var instruction = new Mult();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("mult $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("mult $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("mult $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("mult $t0"));
        Assert.IsFalse(regex.IsMatch("multu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("mult $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Mult")]
    [DataRow(8, 9, 0x01090018)]
    [DataRow(0, 10, 0x000A0018)]
    [DataRow(23, 26, 0x02FA0018)]
    [DataTestMethod("Test assembling")]
    public void MultAssembly(int rs, int rt, int result) {
        var instruction = new Mult {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mult")]
    [DataRow((uint)0x01090018)]
    [DataRow((uint)0x000A0018)]
    [DataRow((uint)0x02FA0018)]
    [DataTestMethod("Test disassembling")]
    public void MultDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mult>(disassembled);
    }

    [TestCategory("Multu")]
    [TestMethod("Test Multu Regex")]
    public void MultuRegex() {
        var instruction = new Multu();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("multu $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("multu $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("multu $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("multu $t0"));
        Assert.IsFalse(regex.IsMatch("mult $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("multu $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Multu")]
    [DataRow(8, 9, 0x01090019)]
    [DataRow(0, 10, 0x000A0019)]
    [DataRow(23, 26, 0x02FA0019)]
    [DataTestMethod("Test assembling")]
    public void MultuAssembly(int rs, int rt, int result) {
        var instruction = new Multu {
            Rs = (byte)rs,
            Rt = (byte)rt,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Multu")]
    [DataRow((uint)0x01090019)]
    [DataRow((uint)0x000A0019)]
    [DataRow((uint)0x02FA0019)]
    [DataTestMethod("Test disassembling")]
    public void MultuDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Multu>(disassembled);
    }

    [TestCategory("Mfhi")]
    [TestMethod("Test Mfhi Regex")]
    public void MfhiRegex() {
        var instruction = new Mfhi();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("mfhi $t0"));
        Assert.IsFalse(regex.IsMatch("mfhi $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("mfhi $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("mfhi"));
        Assert.IsFalse(regex.IsMatch("mflo $t0"));
        Assert.IsFalse(regex.IsMatch("mthi $t0"));
        Assert.IsFalse(regex.IsMatch("mfhi $t0, $t1, $t2, $t3"));
    }

    [TestCategory("Mfhi")]
    [DataRow(31, 0x0000F810)]
    [DataRow(0, 0x00000010)]
    [DataRow(20, 0x0000A010)]
    [DataTestMethod("Test assembling")]
    public void MfhiAssembly(int rd, int result) {
        var instruction = new Mfhi {
            Rd = (byte)rd,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mfhi")]
    [DataRow((uint)0x0000F810)]
    [DataRow((uint)0x00000010)]
    [DataRow((uint)0x0000A010)]
    [DataTestMethod("Test disassembling")]
    public void MfhiDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mfhi>(disassembled);
    }

    [TestCategory("Mflo")]
    [TestMethod("Test Mflo Regex")]
    public void MfloRegex() {
        var instruction = new Mflo();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("mflo $t0"));
        Assert.IsFalse(regex.IsMatch("mflo $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("mflo $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("mflo"));
        Assert.IsFalse(regex.IsMatch("mfhi $t0"));
        Assert.IsFalse(regex.IsMatch("mtlo $t0"));
        Assert.IsFalse(regex.IsMatch("mflo $t0, $t1, $t2, $t3"));
    }

    [TestCategory("Mflo")]
    [DataRow(31, 0x0000F812)]
    [DataRow(0, 0x00000012)]
    [DataRow(20, 0x0000A012)]
    [DataTestMethod("Test assembling")]
    public void MfloAssembly(int rd, int result) {
        var instruction = new Mflo {
            Rd = (byte)rd,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mflo")]
    [DataRow((uint)0x0000F812)]
    [DataRow((uint)0x00000012)]
    [DataRow((uint)0x0000A012)]
    [DataTestMethod("Test disassembling")]
    public void MfloDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mflo>(disassembled);
    }

    [TestCategory("Mthi")]
    [TestMethod("Test Mthi Regex")]
    public void MthiRegex() {
        var instruction = new Mthi();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("mthi $t0"));
        Assert.IsFalse(regex.IsMatch("mthi $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("mthi $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("mthi"));
        Assert.IsFalse(regex.IsMatch("mfhi $t0"));
        Assert.IsFalse(regex.IsMatch("mflo $t0"));
        Assert.IsFalse(regex.IsMatch("mthi $t0, $t1, $t2, $t3"));
    }

    [TestCategory("Mthi")]
    [DataRow(31, 0x03E00011)]
    [DataRow(0, 0x00000011)]
    [DataRow(20, 0x02800011)]
    [DataTestMethod("Test assembling")]
    public void MthiAssembly(int rs, int result) {
        var instruction = new Mthi {
            Rs = (byte)rs,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mthi")]
    [DataRow((uint)0x03E00011)]
    [DataRow((uint)0x00000011)]
    [DataRow((uint)0x02800011)]
    [DataTestMethod("Test disassembling")]
    public void MthiDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mthi>(disassembled);
    }

    [TestCategory("Mtlo")]
    [TestMethod("Test Mtlo Regex")]
    public void MtloRegex() {
        var instruction = new Mtlo();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("mtlo $t0"));
        Assert.IsFalse(regex.IsMatch("mtlo $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("mtlo $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("mtlo"));
        Assert.IsFalse(regex.IsMatch("mfhi $t0"));
        Assert.IsFalse(regex.IsMatch("mflo $t0"));
        Assert.IsFalse(regex.IsMatch("mtlo $t0, $t1, $t2, $t3"));
    }

    [TestCategory("Mtlo")]
    [DataRow(31, 0x03E00013)]
    [DataRow(0, 0x00000013)]
    [DataRow(20, 0x02800013)]
    [DataTestMethod("Test assembling")]
    public void MtloAssembly(int rs, int result) {
        var instruction = new Mtlo {
            Rs = (byte)rs,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Mtlo")]
    [DataRow((uint)0x03E00013)]
    [DataRow((uint)0x00000013)]
    [DataRow((uint)0x02800013)]
    [DataTestMethod("Test disassembling")]
    public void MtloDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Mtlo>(disassembled);
    }

    [TestCategory("Clo")]
    [TestMethod("Test Clo Regex")]
    public void CloRegex() {
        var instruction = new Clo();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("clo $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("clo $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("clo $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("clo $t0"));
        Assert.IsFalse(regex.IsMatch("clz $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("clo $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Clo")]
    [DataRow(31, 27, 0x7360F821)]
    [DataRow(18, 12, 0x71809021)]
    [DataRow(29, 0, 0x7000E821)]
    [DataTestMethod("Test assembling")]
    public void CloAssembly(int rd, int rs, int result) {
        var instruction = new Clo {
            Rd = (byte)rd,
            Rs = (byte)rs,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Clo")]
    [DataRow((uint)0x7360F821)]
    [DataRow((uint)0x71809021)]
    [DataRow((uint)0x7000E821)]
    [DataTestMethod("Test disassembling")]
    public void CloDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Clo>(disassembled);
    }

    [TestCategory("Clz")]
    [TestMethod("Test Clz Regex")]
    public void ClzRegex() {
        var instruction = new Clz();
        Regex? regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("clz $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("clz $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("clz $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("clz $t0"));
        Assert.IsFalse(regex.IsMatch("clo $t0, $t1"));
        Assert.IsFalse(regex.IsMatch("clz $t0, $t1, $t2, $t3, $t4"));
    }

    [TestCategory("Clz")]
    [DataRow(31, 27, 0x7360F820)]
    [DataRow(18, 12, 0x71809020)]
    [DataRow(29, 0, 0x7000E820)]
    [DataTestMethod("Test assembling")]
    public void ClzAssembly(int rd, int rs, int result) {
        var instruction = new Clz {
            Rd = (byte)rd,
            Rs = (byte)rs,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Clz")]
    [DataRow((uint)0x7360F820)]
    [DataRow((uint)0x71809020)]
    [DataRow((uint)0x7000E820)]
    [DataTestMethod("Test disassembling")]
    public void ClzDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Clz>(disassembled);
    }

    [TestCategory("Jalr")]
    [TestMethod("Test Jalr Regex")]
    public void JalrRegex() {
        var instruction1 = new Jalr1();
        var instruction2 = new Jalr2();
        Regex? regex1 = instruction1.GetRegularExpression();
        Regex? regex2 = instruction2.GetRegularExpression();
        Assert.IsTrue(regex2.IsMatch("jalr $t0, $t1"));
        Assert.IsTrue(regex1.IsMatch("jalr $t0"));
        Assert.IsFalse(regex1.IsMatch("jalr $t0, $t1, $t2"));
        Assert.IsFalse(regex2.IsMatch("jalr $t0, $t1, $t2"));
        Assert.IsFalse(regex1.IsMatch("jalr $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex2.IsMatch("jalr $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex1.IsMatch("jr $t0, $t1"));
        Assert.IsFalse(regex2.IsMatch("jr $t0, $t1"));
        Assert.IsFalse(regex1.IsMatch("jalr"));
        Assert.IsFalse(regex2.IsMatch("jalr"));
        Assert.IsFalse(regex1.IsMatch("jalr $t0, $t1"));
        Assert.IsFalse(regex2.IsMatch("jalr $t0"));
    }

    [TestCategory("Jalr")]
    [DataRow(28, 0x0380F809)]
    [DataRow(19, 0x0260F809)]
    [DataTestMethod]
    public void JalrAssemblySingle(int rs, int result) {
        var instruction = new Jalr1 {
            Rs = (byte)rs
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }

    [TestCategory("Jalr")]
    [DataRow(28, 10, 0x0140E009)]
    [DataRow(19, 4, 0x00809809)]
    [DataTestMethod]
    public void JalrAssemblyDouble(int rd, int rs, int result) {
        var instruction = new Jalr2 {
            Rd = (byte)rd,
            Rs = (byte)rs,
        };
        Assert.AreEqual(result, instruction.ConvertToInt());
    }
    
    [TestCategory(("Syscall"))]
    [TestMethod("Test Syscall Assembly")]
    public void SyscallAssembly() {
        var instruction = new Syscall();
        instruction.Code = 0;
        Assert.AreEqual(0x0000000C, instruction.ConvertToInt());
        instruction.Code = 2;
        Assert.AreEqual(0b000000_00000000000000000010_001100, instruction.ConvertToInt());
    }

    [TestCategory("Jalr")]
    [DataRow((uint)0x0380F809)]
    [DataRow((uint)0x0260F809)]
    [DataTestMethod("Test disassembling")]
    public void JalrDisassemblySingle(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Jalr1>(disassembled);
    }

    [TestCategory("Jalr")]
    [DataRow((uint)0x0140E009)]
    [DataRow((uint)0x00809809)]
    [DataTestMethod("Test disassembling")]
    public void JalrDisassemblyDouble(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Jalr2>(disassembled);
    }

    [TestCategory("Syscall")]
    [DataRow((uint)0x0000000C)]
    [DataRow((uint)0b000000_00000000000000000010_001100)]
    [DataTestMethod("Test disassembling")]
    public void SyscallDisassembly(uint instruction) {
        Instruction? disassembled = Disassembler.Disassemble(instruction);
        Assert.IsNotNull(disassembled);
        Assert.IsInstanceOfType<Syscall>(disassembled);
    }

    
}