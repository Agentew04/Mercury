using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(25,21,0)]
[FormatExact(5,0,0)]
[FormatDifferent(10,6,0)]
public partial class Sll : IInstruction {

    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,11)]
    public byte Rd { get; set; }
    
    [Field(10,6)]
    public byte ShiftAmount { get; set; }
    
    public override string ToString() => $"sll ${Instruction.TranslateRegisterName(Rd)}, ${Instruction.TranslateRegisterName(Rt)}, {ShiftAmount}";
}
