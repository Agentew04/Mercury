using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)] // opcode
[FormatExact(25,21,0)] // rs
[FormatExact(5,0,2)] // funct
public partial class Srl : IInstruction {

    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,11)]
    public byte Rd { get; set; }
    
    [Field(10,6)]
    public byte ShiftAmount { get; set; }
    
    public override string ToString() => $"srl ${Instruction.TranslateRegisterName(Rd)}, ${Instruction.TranslateRegisterName(Rt)}, {ShiftAmount}";
}
