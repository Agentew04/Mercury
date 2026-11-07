using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,26,0)] // opcode
[FormatExact(25,21,0)] // rs
[FormatExact(5,0,3)] // funct
[AssemblyFormat("sra ${Rd:reg}, ${Rt:reg}, {ShiftAmount}")]
public partial class Sra : IInstruction {
    
    [Field(20,16)]
    public byte Rt { get; set; }
    
    [Field(15,11)]
    public byte Rd { get; set; }
    
    [Field(10,6)]
    public short ShiftAmount { get; set; }
}
