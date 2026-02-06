using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

/// <summary>
/// Jump and link register. Jumps to the address stored in the register provided and stores the return address in $ra register or
/// an specific one.
/// </summary>
[Instruction]
[FormatExact(31,26,0)]
[FormatExact(20,16,0)]
[FormatExact(10,6,0)]
[FormatExact(5,0,9)]
public partial class Jalr : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(15,11)]
    public byte Rd { get; set; }
    
    public override string ToString() => $"jalr ${Instruction.TranslateRegisterName(Rd)}, ${Instruction.TranslateRegisterName(Rs)}";
}
