using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.RiscV.RV32I.Instructions;

[Instruction]
[FormatExact(31,7,0)]
[FormatExact(6,2,0b11100)]
[FormatExact(1,0,0b11)]
public partial class Ecall : IInstruction;