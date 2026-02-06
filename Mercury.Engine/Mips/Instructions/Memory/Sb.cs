using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,40)] // opcode
public partial class Sb : IInstruction {

    [Field(25,21)]
    public byte Base { get; set; }
    
    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,0)]
    public short Offset { get; set; }
    
    public override string ToString() => $"sb ${Instruction.TranslateRegisterName(Rt)}, {Offset}(${Instruction.TranslateRegisterName(Base)})";
}
