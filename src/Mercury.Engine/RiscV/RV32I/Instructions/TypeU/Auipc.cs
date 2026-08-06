using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.RiscV.RV32I.Instructions;

[Instruction]
[FormatExact(6,2,0b00101)]
[FormatExact(1,0,0b11)]
public partial class Auipc : IInstruction
{
    [Field(11,7)]
    public byte Rd { get; set; }
    
    [Field(31,12)]
    public int Immediate { get; set; }

    public override string ToString() => $"auipc {Instruction.TranslateRegisterName(Rd)} {Immediate:X8}";
}