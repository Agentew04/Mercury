using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)] // opcode
[FormatExact(5,0,12)] // funct
public partial class Syscall : IInstruction {

    [Field(25,6)]
    public int Code { get; set; }

    public override string ToString() => $"syscall {(Code == 0 ? "" : Code.ToString())}";
}
