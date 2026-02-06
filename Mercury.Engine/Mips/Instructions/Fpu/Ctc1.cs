using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,17)]
[FormatExact(25,21,6)]
[FormatExact(10,6,0)]
[FormatExact(5,0,9)]
public partial class Ctc1 : IInstruction {
    
    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,11)]
    public byte Fs { get; set; }

    public override string ToString() => $"ctc1 ${Instruction.TranslateRegisterName(Rt)}, ${TypeFInstruction.TranslateRegisterName(Fs)}";

}