using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

/// <summary>
/// Trap if equal. Triggers a breakpoint if the contents of Rs and Rt are equal.
/// </summary>
[Instruction]
[FormatExact(31,26,0)]
[FormatExact(5,0,52)]
public partial class Teq : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,6)]
    public short Code { get; set; }
    
    public override string ToString() => $"teq ${Instruction.TranslateRegisterName(Rs)}, ${Instruction.TranslateRegisterName(Rt)}";
}
