using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(15,0,24)]
public partial class Mult : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(20,16)]
    public byte Rt { get; set; }
    
    public override string ToString() => $"mult ${Instruction.TranslateRegisterName(Rs)}, ${Instruction.TranslateRegisterName(Rt)}";
}
