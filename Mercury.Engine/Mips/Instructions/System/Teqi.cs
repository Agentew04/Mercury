using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,1)] // opcode
[FormatExact(20,16,12)] // rt
public partial class Teqi : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(15,0)]
    public short Immediate { get; set; }
    
    public override string ToString() => $"teqi ${Instruction.TranslateRegisterName(Rs)}, {Immediate}";
}
