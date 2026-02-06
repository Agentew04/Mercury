using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

/// <summary>
/// Jump to register instruction. Jumps to the address stored in the register provided.
/// </summary>
[Instruction]
[FormatExact(31,26,0)]
[FormatExact(20,6,0)]
[FormatExact(5,0,8)]
public partial class Jr : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    public override string ToString() => $"jr ${Instruction.TranslateRegisterName(Rs)}";
}
