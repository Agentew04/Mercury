namespace Mercury.Engine.Mips.Instructions; 

public static class Instruction {
    
    private static readonly string[] Names = [
        "zero", "at", "v0", "v1", "a0", "a1", "a2", "a3", "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7", 
        "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7", "t8", "t9", "k0", "k1", "gp", "sp", "fp", "ra"
    ];
    
    [global::Mercury.Engine.Generators.Instruction.AssemblyFormatter("reg")]
    public static string TranslateRegisterName(int index)
    {
        return Names[index];
    }
    
    public const byte SinglePrecisionFormat = 0b10000;
    public const byte DoublePrecisionFormat = 0b10001;//10100
    public const byte WordFixedPrecisionFormat = 0b10100;
    public const byte LongFixedPrecisionFormat = 0b10101;
    
    [global::Mercury.Engine.Generators.Instruction.AssemblyFormatter("fpreg")]
    public static string FpuTranslateRegisterName(int index)
    {
        return $"f{index}";
    }

    [global::Mercury.Engine.Generators.Instruction.AssemblyFormatter("fmt")]
    public static string FpuFormatFmt(byte fmt)
    {
        return fmt switch
        {
            SinglePrecisionFormat => "s",
            DoublePrecisionFormat => "d",
            WordFixedPrecisionFormat => "w",
            _ => throw new ArgumentOutOfRangeException(nameof(fmt), "Invalid format code. Got: " + fmt)
        };
    }
}
