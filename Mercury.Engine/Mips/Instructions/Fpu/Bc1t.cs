using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,17)]
[FormatExact(25,21,8)]
[FormatExact(17,17,0)]
[FormatExact(16,16,1)]
public partial class Bc1T : IInstruction {
    
    [Field(20,18)]
    public byte Cc { get; set; }
    
    [Field(15,0)]
    public short Offset { get; set; }

    public override string ToString() => $"bc1f {Offset<<2:X4}";

}