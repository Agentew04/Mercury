using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,17)]
[FormatExact(20,16,0)]
[FormatExact(25,21,[16,17,20])]
[FormatExact(5,0,7)]
public partial class Neg : IInstruction {

    [Field(25,21)]
    public byte Fmt { get; set; }

    [Field(15,11)]
    public byte Fs { get; set; }

    [Field(10,6)]
    public byte Fd { get; set; }

    public bool IsDouble => Fmt == TypeFInstruction.DoublePrecisionFormat;

    public override string ToString() => $"neg.{TypeFInstruction.FormatFmt(Fmt)} ${TypeFInstruction.TranslateRegisterName(Fd)}, ${TypeFInstruction.TranslateRegisterName(Fs)}";
}