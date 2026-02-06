using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,1)]
[FormatExact(20,16,0)]
public partial class Bltz : IInstruction{

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(15,0)]
    public short Offset { get; set; }
    
    public override string ToString() => $"bltz ${Instruction.TranslateRegisterName(Rs)}, 0x{Offset:X4}";
}
