using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,28)]
[FormatExact(20,16,0)]
[FormatExact(10,6,0)]
[FormatExact(5,0,32)]
public partial class Clz : IInstruction {

    [Field(25,21)]
    public byte Rs { get; set; }
    
    [Field(15,11)]
    public byte Rd { get; set; }
    
    public override string ToString() => $"clz ${Instruction.TranslateRegisterName(Rd)}, ${Instruction.TranslateRegisterName(Rs)}";
}
