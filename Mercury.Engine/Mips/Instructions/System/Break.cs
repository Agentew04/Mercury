using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)]
[FormatExact(5,0,13)]
public partial class Break : IInstruction{

    [Field(25,6)]
    public int Code { get; set; }
    
    public override string ToString() => $"break 0x{Code:X5}";
}
