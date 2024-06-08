namespace SAAE.Engine.Test.Mips;

[TestClass]
public class InstructionsTestTypeR
{
    [TestCategory("Add")]
    [TestMethod("Test Regex matching")]
    public void AddRegex()
    {
        var instruction = new Engine.Mips.Instructions.Add();
        var regex = instruction.GetRegularExpression();
        Assert.IsTrue(regex.IsMatch("add $t0, $t1, $t2"));
        Assert.IsTrue(regex.IsMatch("add $t0, $t1, $t2"));
        Assert.IsTrue(regex.IsMatch("add $t0, $t1, $t2"));
        Assert.IsFalse(regex.IsMatch("add $t0, $t1, $t2, $t3"));
        Assert.IsFalse(regex.IsMatch("add $t0, $t1"));
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

    [TestCategory]
}