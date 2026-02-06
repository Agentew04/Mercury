using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,8)]
public partial class Addi : IInstruction {

    [Field(26,21)]
    public byte Rs { get; set; }
    
    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,0)]
    public short Immediate { get; set; }

    public override string ToString() =>
        $"addi ${Instruction.TranslateRegisterName(Rt)}, ${Instruction.TranslateRegisterName(Rs)}, {Immediate}";
}
