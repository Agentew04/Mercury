using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,28,4)]
public partial class Copz : IInstruction{
    
    [Field(27,26)]
    public byte Coprocessor { get; set; }
    
    [Field(25,0)]
    public int Function { get; set; }

    public string ToString() => $"cop{Coprocessor} {Function:X7}";
}