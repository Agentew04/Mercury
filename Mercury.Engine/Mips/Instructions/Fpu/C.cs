using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,17)]
[FormatExact(25,21,[16,17,20])]
[FormatExact(7,4,3)]
public partial class C : IInstruction {
    
    [Field(25,21)]
    public byte Format { get; set; }
    
    [Field(20,16)]
    public byte Ft { get; set; }
    
    [Field(15,11)]
    public byte Fs { get; set; }
    
    [Field(10,8)]
    public byte Cc { get; set; }
    
    [Field(3,0)]
    public byte Cond { get; set; }

    public bool IsDouble => Format == TypeFInstruction.DoublePrecisionFormat;
    
    public override string ToString()
    {
        return $"C.{FormatCond(Cond)}.{TypeFInstruction.FormatFmt(Format)} {(Cc!=0?$"{Cc}, ":"")}${TypeFInstruction.TranslateRegisterName(Fs)}, ${TypeFInstruction.TranslateRegisterName(Ft)}";
    }

    private static string FormatCond(byte value)
    {
        return value switch
        {
            0b0000 => "f",
            0b0001 => "un",
            0b0010 => "eq",
            0b0011 => "ueq",
            0b0100 => "olt",
            0b0101 => "ult",
            0b0110 => "ole",
            0b0111 => "ule",
            0b1000 => "sf",
            0b1001 => "ngle",
            0b1010 => "seq",
            0b1011 => "ngl",
            0b1100 => "lt",
            0b1101 => "nge",
            0b1110 => "le",
            0b1111 => "ngt",
            _ => "unknown"
        };
    }
}
