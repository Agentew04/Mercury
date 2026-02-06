using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(10,0,36)]
public partial class And : IInstruction{

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,11)]
    public byte Rd { get; set; }

    public override string ToString() => $"and ${Instruction.TranslateRegisterName(Rd)}, ${Instruction.TranslateRegisterName(Rs)}, ${Instruction.TranslateRegisterName(Rt)}";
}
