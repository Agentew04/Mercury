using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(5,0,48)]
public partial class Tge : IInstruction {
    
    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,6)]
    public short Code { get; set; }

    public override string ToString() => $"tge ${Instruction.TranslateRegisterName(Rs)}, ${Instruction.TranslateRegisterName(Rt)}";
}