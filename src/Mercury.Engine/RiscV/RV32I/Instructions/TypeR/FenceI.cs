using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.RiscV.RV32I.Instructions;

[Instruction]
[FormatExact(31,15,0)]
[FormatExact(14,12,0b001)]
[FormatExact(11,7,0)]
[FormatExact(6,2,0b00011)]
[FormatExact(1,0,0b11)]
public partial class FenceI : IInstruction;