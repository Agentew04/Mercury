using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(20,0,17)]
public partial class Mthi : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    public override string ToString() => $"mthi ${Instruction.TranslateRegisterName(Rs)}";
}
