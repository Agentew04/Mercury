using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,17)] // opcode
[FormatExact(20,16,0)] // rt
[FormatExact(25,21,[16,17,20])] // rs
[FormatExact(5,0,33)] // funct
public partial class CvtD : IInstruction {
    
    [Field(25,21)]
    public byte Format { get; set; }
    
    [Field(15,11)]
    public byte Fs { get; private set; }
    
    [Field(10,6)]
    public byte Fd { get; private set; }

    public bool IsDouble => Format == TypeFInstruction.DoublePrecisionFormat;
    
    public override string ToString() => $"cvt.d.{TypeFInstruction.FormatFmt(Format)} ${TypeFInstruction.TranslateRegisterName(Fd)}, ${TypeFInstruction.TranslateRegisterName(Fs)}";

}