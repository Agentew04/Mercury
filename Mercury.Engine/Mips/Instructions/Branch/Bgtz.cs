using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,7)]
[FormatExact(20,16,0)]
public partial class Bgtz : IInstruction{

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(15,0)]
    public short Immediate { get; set; }

    public override string ToString() => $"bgtz ${Instruction.TranslateRegisterName(Rs)}, 0x{Immediate:X4}";
}
