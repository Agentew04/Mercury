using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(10,6,0)]
[FormatExact(5,0,4)]
public partial class Sllv : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,11)]
    public byte Rd { get; set; }
    
    public override string ToString() => $"sllv ${Instruction.TranslateRegisterName(Rd)}, ${Instruction.TranslateRegisterName(Rt)}, ${Instruction.TranslateRegisterName(Rs)}";
}
