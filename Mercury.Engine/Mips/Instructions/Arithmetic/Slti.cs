using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,10)] // opcode
public partial class Slti : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,0)]
    public short Immediate { get; set; }

    public override string ToString() =>
        $"slti ${Instruction.TranslateRegisterName(Rt)}, ${Instruction.TranslateRegisterName(Rs)}, {Immediate}";
}
