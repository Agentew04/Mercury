using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,17)]
[FormatExact(25,21,[16,17,20])]
[FormatExact(5,0,0)]
public partial class AddFloat : IInstruction {
    
    [Field(25,21)]
    public byte Format { get; set; }
    
    [Field(20,16)]
    public byte Ft { get; private set; }
    
    [Field(15,11)]
    public byte Fs { get; private set; }
    
    [Field(10,6)]
    public byte Fd { get; private set; }
    
    public bool IsDouble => Format == TypeFInstruction.DoublePrecisionFormat;

    public override string ToString() => $"add.{TypeFInstruction.FormatFmt(Format)} ${TypeFInstruction.TranslateRegisterName(Fd)}, ${TypeFInstruction.TranslateRegisterName(Fs)}, ${TypeFInstruction.TranslateRegisterName(Ft)}";

}
