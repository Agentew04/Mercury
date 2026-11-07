using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.Mips.Instructions;

[Instruction]
[FormatExact(31,28,12)]
[AssemblyFormat("lwc{Coprocessor} ${Ft:fpreg}, {Offset:X4}(${Base:fpreg})")]
public partial class Lwcz : IInstruction {
    
    [Field(27,26)]
    public byte Coprocessor { get; set; }
    
    [Field(25,21)]
    public byte Base { get; set; }
    
    [Field(20,16)]
    public byte Ft { get; set; }
    
    [Field(15,0)]
    public short Offset { get; set; }
}